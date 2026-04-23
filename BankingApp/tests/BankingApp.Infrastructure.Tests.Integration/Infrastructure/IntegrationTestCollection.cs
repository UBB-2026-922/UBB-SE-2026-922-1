// Copyright (c) BankingApp. All rights reserved.
// Licensed under the MIT license.

namespace BankingApp.Infrastructure.Tests.Integration.Infrastructure;

[CollectionDefinition("Integration")]
public sealed class IntegrationTestCollection : ICollectionFixture<DatabaseFixture>
{
}