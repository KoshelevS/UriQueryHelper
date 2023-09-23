using UriQueryHelper;

namespace UriQueryHelperTests;

public class UriQueryTests
{
    private static readonly UriQuery target = new();

    [TestCase("")]
    [TestCase("?")]
    public void ParsesEmptyQuery(string query)
    {
        Assert.That(target.Parse(query), Is.Empty);
    }

    [TestCase("param=value")]
    [TestCase("?param=value")]
    public void ParsesSingleParameterWithValue(string query)
    {
        var actual = target.Parse(query);

        Assert.That(actual, Does.ContainKey("param").WithValue(new[] { "value" }));
        Assert.That(actual, Has.One.Items);
    }

    [Test]
    public void ParsesSingleValuelessParameter()
    {
        var actual = target.Parse("?param=");

        Assert.That(actual, Does.ContainKey("param").WithValue(new[] { "" }));
        Assert.That(actual, Has.One.Items);
    }

    [Test]
    public void ParsesMultipleParametersWithValues()
    {
        var actual = target.Parse("?param1=value1&param2=value2&param3=value3");

        Assert.That(actual, Does.ContainKey("param1").WithValue(new[] { "value1" }));
        Assert.That(actual, Does.ContainKey("param2").WithValue(new[] { "value2" }));
        Assert.That(actual, Does.ContainKey("param3").WithValue(new[] { "value3" }));
        Assert.That(actual, Has.Exactly(3).Items);
    }

    [Test]
    public void ParsesMultivaluedParameter_SimpleSyntax()
    {
        var actual = target.Parse("?param=value1&param=value2&param=value3");

        Assert.That(actual, Does.ContainKey("param").WithValue(new[] { "value1", "value2", "value3" }));
        Assert.That(actual, Has.One.Items);
    }

    [Test]
    public void ParsesMultivaluedParameter_SquareBracketsSyntax()
    {
        var actual = target.Parse("?param[]=value1&param[]=value2&param[]=value3");

        Assert.That(actual, Does.ContainKey("param").WithValue(new[] { "value1", "value2", "value3" }));
        Assert.That(actual, Has.One.Items);
    }

    [TestCase("?param", "param")]
    [TestCase("?param1&param2=value2", "param1")]
    [TestCase("?param1=value1&param2", "param2")]
    [TestCase("?=value", "=value")]
    [TestCase("?=value1&param2=value2", "=value1")]
    [TestCase("?param1=value1&=value2", "=value2")]
    public void ParsesReportsInvalidParameters(string query, string invalidExpression)
    {
        var exception = Assert.Throws<ArgumentException>(() => target.Parse(query));
        Assert.That(
            exception.Message,
            Is.EqualTo($"'{invalidExpression}' is not a valid parameter expression (Parameter 'query')"));
    }

    [Test]
    public void CannotParseNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => target.Parse(null!));
        Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'query')"));
    }

    [Test]
    public void DecodesSpecialCharacters()
    {
        Assert.That(
            target.Parse("?param%21=%20%21%22%23%24%25%26%27%28%29%2A%2B%2C%2F%3A%3B%3D%3F%40%5B%5D"),
            Does.ContainKey("param!").WithValue(new[] { " !\"#$%&'()*+,/:;=?@[]" }));
    }

    [Test]
    public void SerializesEmptyParameters()
    {
        var parameters = new Dictionary<string, List<string>>();
        Assert.That(target.Serialize(parameters), Is.EqualTo("?"));
    }

    [Test]
    public void SerializesOneParameterValue()
    {
        var parameters = new Dictionary<string, List<string>>
        {
            ["param"] = new() { "value" }
        };
        Assert.That(target.Serialize(parameters), Is.EqualTo("?param=value"));
    }

    [Test]
    public void SerializesOneValuelessParameter()
    {
        var parameters = new Dictionary<string, List<string>>
        {
            ["param"] = new()
        };
        Assert.That(target.Serialize(parameters), Is.EqualTo("?param="));
    }

    [Test]
    public void SerializesOneMultivalueParameter()
    {
        var parameters = new Dictionary<string, List<string>>
        {
            ["param"] = new() { "value1", "value2", "value3" }
        };

        Assert.That(
            target.Serialize(parameters),
            Is.EqualTo("?param[]=value1&param[]=value2&param[]=value3"));
    }

    [Test]
    public void SerializesMultipleParameters()
    {
        var parameters = new Dictionary<string, List<string>>
        {
            ["param1"] = new() { "value1" },
            ["param2"] = new() { "value2" },
            ["param3"] = new() { "value3" },
        };

        Assert.That(
            target.Serialize(parameters),
            Is.EqualTo("?param1=value1&param2=value2&param3=value3"));
    }

    [Test]
    public void CannotSerializeNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => target.Parse(null!));
        Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'query')"));
    }

    [Test]
    public void EncodesSpecialCharacters()
    {
        var parameters = new Dictionary<string, List<string>>
        {
            ["param!"] = new() { " !\"#$%&'()*+,/:;=?@[]" },
        };

        Assert.That(
            target.Serialize(parameters),
            Is.EqualTo("?param%21=%20%21%22%23%24%25%26%27%28%29%2A%2B%2C%2F%3A%3B%3D%3F%40%5B%5D"));
    }
}
