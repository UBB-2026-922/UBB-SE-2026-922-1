#!/usr/bin/env python3
"""Generate local development env files for BankingApp."""

from __future__ import annotations

import argparse
import base64
import secrets
import shutil
import string
import subprocess
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
APP_ROOT = SCRIPT_DIR.parents[1]
API_PROJECT = APP_ROOT / "src" / "BankingApp.Api"
COMPOSE_ENV_FILE = APP_ROOT / ".env"
API_ENV_FILE = API_PROJECT / ".env"

SQL_PASSWORD_LENGTH = 24
JWT_SECRET_BYTES = 48
OTP_SECRET_BYTES = 32

DOCKER_CONNECTION_STRING = (
    "Server=db;Database=BankingAppDb;User Id=sa;"
    "Password={password};TrustServerCertificate=True;"
)

DOCKER_HOST_CONNECTION_STRING = (
    "Server=localhost,1433;Database=BankingAppDb;User Id=sa;"
    "Password={password};TrustServerCertificate=True;"
)

LOCAL_CONNECTION_STRING = (
    "Server=localhost;Database=BankingAppDb;"
    "Trusted_Connection=True;TrustServerCertificate=True;"
)

PLACEHOLDER_SMTP_HOST = "smtp.example.com"
PLACEHOLDER_SMTP_USER = "dev@example.com"
PLACEHOLDER_SMTP_PASS = "placeholder"


def generate_sql_password(length: int = SQL_PASSWORD_LENGTH) -> str:
    """Generate a SQL Server-compatible password."""
    special_chars = "!@#$%^&*"
    alphabet = string.ascii_letters + string.digits + special_chars

    while True:
        password = "".join(secrets.choice(alphabet) for _ in range(length))
        if (
            any(char.isupper() for char in password)
            and any(char.islower() for char in password)
            and any(char.isdigit() for char in password)
            and any(char in special_chars for char in password)
        ):
            return password


def generate_base64_secret(byte_count: int) -> str:
    """Generate a base64-encoded random secret."""
    return base64.b64encode(secrets.token_bytes(byte_count)).decode("ascii")


def read_env_file(path: Path) -> dict[str, str]:
    """Read a simple KEY=VALUE env file."""
    values: dict[str, str] = {}
    if not path.exists():
        return values

    for line in path.read_text(encoding="utf-8-sig").splitlines():
        stripped = line.strip()
        if not stripped or stripped.startswith("#") or "=" not in stripped:
            continue
        key, _, value = stripped.partition("=")
        values[key.strip()] = value.strip()

    return values


def write_env_file(path: Path, values: dict[str, str], header: str) -> None:
    """Write an env file with stable key ordering."""
    lines = [header, ""]
    lines.extend(f"{key}={value}" for key, value in values.items())
    lines.append("")
    path.write_text("\n".join(lines), encoding="utf-8")


def ensure_can_write(path: Path, force: bool) -> None:
    """Refuse to overwrite existing files unless force is enabled."""
    if path.exists() and not force:
        raise SystemExit(f"{path} already exists. Re-run with --force to overwrite it.")


def set_user_secret(key: str, value: str) -> None:
    """Set one .NET user secret for the API project."""
    result = subprocess.run(
        ["dotnet", "user-secrets", "set", "--project", str(API_PROJECT), key, value],
        capture_output=True,
        text=True,
        check=False,
    )

    if result.returncode != 0:
        message = result.stderr.strip() or result.stdout.strip()
        raise SystemExit(f"Failed to set user secret {key}: {message}")


def parse_args() -> argparse.Namespace:
    """Parse command-line arguments."""
    parser = argparse.ArgumentParser(description="Generate local BankingApp development env files.")
    parser.add_argument("--force", action="store_true", help="Overwrite existing generated env files.")
    parser.add_argument(
        "--db-mode",
        choices=["docker", "docker-host", "local"],
        default="docker",
        help=(
            "Database connection target for API config. "
            "'docker' is for API in Compose, 'docker-host' is for API in an IDE with DB in Compose, "
            "and 'local' is for a trusted local SQL Server. Defaults to docker."
        ),
    )
    parser.add_argument(
        "--db-password",
        default=None,
        help="SQL Server SA password for Docker mode. Generated when omitted.",
    )
    parser.add_argument("--smtp-host", default=PLACEHOLDER_SMTP_HOST, help="SMTP host.")
    parser.add_argument("--smtp-port", default="587", help="SMTP port.")
    parser.add_argument("--smtp-user", default=None, help="SMTP username.")
    parser.add_argument("--smtp-pass", default=None, help="SMTP password.")
    parser.add_argument("--smtp-from", default=None, help="SMTP sender address.")
    parser.add_argument(
        "--user-secrets",
        action="store_true",
        help="Also write the same API values to .NET User Secrets for dotnet run.",
    )
    return parser.parse_args()


def main() -> None:
    """Generate development configuration."""
    args = parse_args()

    ensure_can_write(COMPOSE_ENV_FILE, args.force)
    ensure_can_write(API_ENV_FILE, args.force)

    existing_compose_env = read_env_file(COMPOSE_ENV_FILE)
    db_password = (
        args.db_password
        or existing_compose_env.get("DB_SA_PASSWORD")
        or generate_sql_password()
    )

    smtp_missing = not args.smtp_user or not args.smtp_pass
    smtp_user = args.smtp_user or PLACEHOLDER_SMTP_USER
    smtp_pass = args.smtp_pass or PLACEHOLDER_SMTP_PASS
    smtp_from = args.smtp_from or smtp_user

    if args.db_mode == "docker":
        connection_string = DOCKER_CONNECTION_STRING.format(password=db_password)
    elif args.db_mode == "docker-host":
        connection_string = DOCKER_HOST_CONNECTION_STRING.format(password=db_password)
    else:
        connection_string = LOCAL_CONNECTION_STRING

    compose_values = {
        "DB_SA_PASSWORD": db_password,
    }
    api_values = {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ConnectionStrings__BankingAppDb": connection_string,
        "Jwt__Secret": generate_base64_secret(JWT_SECRET_BYTES),
        "Otp__Secret": generate_base64_secret(OTP_SECRET_BYTES),
        "Email__SmtpHost": args.smtp_host,
        "Email__SmtpPort": args.smtp_port,
        "Email__SmtpUser": smtp_user,
        "Email__SmtpPass": smtp_pass,
        "Email__FromAddress": smtp_from,
        "Database__ApplyMigrations": "true",
    }

    write_env_file(
        COMPOSE_ENV_FILE,
        compose_values,
        "# Generated by scripts/secrets/setup_dev.py. Do not commit.",
    )
    write_env_file(
        API_ENV_FILE,
        api_values,
        "# Generated by scripts/secrets/setup_dev.py. Do not commit.",
    )

    if args.user_secrets:
        if shutil.which("dotnet") is None:
            raise SystemExit("dotnet was not found on PATH, so user secrets cannot be written.")

        user_secret_keys = {
            "ConnectionStrings:BankingAppDb": api_values["ConnectionStrings__BankingAppDb"],
            "Jwt:Secret": api_values["Jwt__Secret"],
            "Otp:Secret": api_values["Otp__Secret"],
            "Email:SmtpHost": api_values["Email__SmtpHost"],
            "Email:SmtpPort": api_values["Email__SmtpPort"],
            "Email:SmtpUser": api_values["Email__SmtpUser"],
            "Email:SmtpPass": api_values["Email__SmtpPass"],
            "Email:FromAddress": api_values["Email__FromAddress"],
            "Database:ApplyMigrations": api_values["Database__ApplyMigrations"],
        }
        for key, value in user_secret_keys.items():
            set_user_secret(key, value)

    print("Generated development configuration:")
    print(f"  {COMPOSE_ENV_FILE.relative_to(APP_ROOT)}")
    print(f"  {API_ENV_FILE.relative_to(APP_ROOT)}")
    if args.user_secrets:
        print("  .NET User Secrets for src/BankingApp.Api")
    if smtp_missing:
        print("Warning: placeholder SMTP values were written. Email sending will fail until configured.")
    print("\nNext step:")
    print("  docker compose up --build")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        sys.exit(130)
