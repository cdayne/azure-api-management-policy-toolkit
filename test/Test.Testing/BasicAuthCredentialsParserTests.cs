// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

using Microsoft.Azure.ApiManagement.PolicyToolkit.Authoring.Expressions;

namespace Microsoft.Azure.ApiManagement.PolicyToolkit.Testing.Expressions.Extensions;

[TestClass]
public class BasicAuthCredentialsParserTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow(" ")]
    [DataRow("Digest value")]
    [DataRow("Basic ")]
    [DataRow("Basic notBase64Encoded")]
    public void BasicAuthCredentialsParser_ShouldReturnNull(string value)
    {
        // Act
        var result = new BasicAuthCredentialsParser().Parse(value);

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public void MockBasicAuthCredentials_ShouldKeepUsernameAsPositionalParameter()
    {
        // Arrange
#pragma warning disable CS0618
        var credentials = new MockBasicAuthCredentials(Username: "user", Password: "password");

        // Act
        var renamed = credentials with { Username = "other" };
        var (username, password) = credentials;
#pragma warning restore CS0618

        // Assert
        credentials.UserId.Should().Be("user");
        renamed.UserId.Should().Be("other");
        (credentials with { UserId = "id" }).UserId.Should().Be("id");
        username.Should().Be("user");
        password.Should().Be("password");
    }

    [TestMethod]
    public void BasicAuthCredentials_ShouldDefaultUserIdToUsernameOfExistingImplementations()
    {
        // Arrange
        BasicAuthCredentials credentials = new LegacyCredentials();

        // Act
        var userId = credentials.UserId;

        // Assert
        userId.Should().Be("user");
    }

    private sealed class LegacyCredentials : BasicAuthCredentials
    {
        public string Username => "user";
        public string Password => "password";
    }

    [TestMethod]
    public void BasicAuthCredentialsParser_ShouldReturnFilledObject()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("username:password"));
        var headerValue = $"Basic {credentials}";

        // Act
        var result = new BasicAuthCredentialsParser().Parse(headerValue);

        // Assert
        result.Should().NotBeNull();
        result!.Password.Should().Be("password");
        result!.UserId.Should().Be("username");
#pragma warning disable CS0618 // The obsolete Username still returns UserId.
        result.Username.Should().Be("username");
#pragma warning restore CS0618
    }

    [TestMethod]
    public void MockBasicAuthCredentials_ShouldExposeObsoleteUsername()
    {
        var credentials = new MockBasicAuthCredentials("user", "password");

#pragma warning disable CS0618 // The obsolete Username still returns UserId.
        credentials.Username.Should().Be("user");
#pragma warning restore CS0618
    }

    [TestMethod]
    public void BasicAuthCredentialsParser_ShouldSplitOnLastDoubleDot()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("user:name:password"));
        var headerValue = $"Basic {credentials}";

        // Act
        var result = new BasicAuthCredentialsParser().Parse(headerValue);

        // Assert
        result.Should().NotBeNull();
        result!.Password.Should().Be("password");
        result!.UserId.Should().Be("user:name");
    }

    [TestMethod]
    public void BasicAuthCredentialsParser_ShouldHandleIso_8859_1Encoding()
    {
        // Arrange
        var credentials = Convert.ToBase64String(Encoding.GetEncoding("iso-8859-1").GetBytes("¡usernameÿ:password"));
        var headerValue = $"Basic {credentials}";

        // Act
        var result = new BasicAuthCredentialsParser().Parse(headerValue);

        // Assert
        result.Should().NotBeNull();
        result!.Password.Should().Be("password");
        result!.UserId.Should().Be("¡usernameÿ");
    }
}