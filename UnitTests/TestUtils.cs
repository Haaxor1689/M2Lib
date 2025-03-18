using JsonDiffPatchDotNet;
using M2Lib.m2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnitTests
{
    public static class TestUtils
    {
        private class DiffLine
        {
            public string Name { get; set; }
            public string Text { get; set; }
            public bool Removed { get; set; }
            public bool Added { get; set; }
        }

        static readonly string TestDataPath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
            "TestData"
        );

        public static M2 LoadModel(string name)
        {
            var model = new M2();
            var filePath = Path.Combine(TestDataPath, name);
            using (var reader = new BinaryReader(new FileStream(filePath, FileMode.Open)))
                model.Load(reader);
            return model;
        }

        public static void SaveModel(string name, M2 model)
        {
            var filePath = Path.Combine(TestDataPath, name);
            using var writer = new BinaryWriter(new FileStream(filePath, FileMode.Create));
            model.Save(writer);
        }

        public static void SaveJson(string name, M2 model)
        {
            var filePath = Path.Combine(TestDataPath, name);
            var jsonString = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
        }

        private static string GetFormattedJsonDiff<T>(T obj1, T obj2)
        {
            var json1 = JsonConvert.SerializeObject(obj1, Formatting.Indented);
            var json2 = JsonConvert.SerializeObject(obj2, Formatting.Indented);
            var diff = new JsonDiffPatch().Diff(JToken.Parse(json1), JToken.Parse(json2));

            if (diff == null)
                return string.Empty;

            return FormatRecursive(diff);
        }

        private static bool IsRoundErrorFloat(JToken token)
        {
            if (token.Type != JTokenType.Array || token.Children().Count() != 2)
                return false;

            var from = token.Children().First();
            var to = token.Children().Last();
            var fromVal = Math.Round(from.Value<float>(), 1);
            var toVal = Math.Round(to.Value<float>(), 1);
            return fromVal == toVal;
        }

        private static string FormatRecursive(JToken token, string indent = "")
        {
            var result = string.Empty;
            switch (token.Type)
            {
                case JTokenType.Object:
                    var isArray =
                        token.First is JProperty type
                        && type.Name == "_t"
                        && type.Value.ToString() == "a";

                    var lines = new List<DiffLine>();
                    foreach (var prop in token.Children<JProperty>().Skip(isArray ? 1 : 0))
                    {
                        var unchanged = IsRoundErrorFloat(prop.Value);
                        if (unchanged)
                            continue;

                        var text = FormatRecursive(prop.Value, indent + "  ");
                        if (text == string.Empty)
                            continue;

                        var removed =
                            prop.Value.Type == JTokenType.Array
                            && prop.Value.Children().Count() == 3;
                        var added =
                            prop.Value.Type == JTokenType.Array
                            && prop.Value.Children().Count() == 1;

                        lines.Add(
                            new()
                            {
                                Name = prop.Name,
                                Text = text,
                                Removed = removed,
                                Added = added,
                            }
                        );
                    }

                    if (lines.Count == 0)
                        return string.Empty;

                    result += isArray ? "[\n" : "{\n";
                    foreach (var l in lines)
                    {
                        result += indent;
                        result +=
                            l.Added ? $" +{l.Name}"
                            : l.Removed ? $" -{l.Name[(isArray ? 1 : 0)..]}"
                            : $"  {l.Name}";
                        result += $": {l.Text}\n";
                    }
                    result += isArray ? $"{indent}]" : $"{indent}}}";

                    break;
                case JTokenType.Array:
                    if (!token.Children().Any())
                        result += "???";
                    else if (token.Children().Count() == 2)
                        result += string.Join(" -> ", token.Children());
                    else
                        result += FormatRecursive(token.Children().First(), indent + "  ");

                    break;
                default:
                    result += token.ToString();
                    break;
            }
            return result;
        }

        private static readonly string TmpPath = Path.Combine(TestDataPath, "tmp");
        private static readonly string OutPath = Path.Combine(TestDataPath, "out");

        public static void AssertDeepEqual<T>(string name, T obj1, T obj2)
        {
            var diff = GetFormattedJsonDiff(obj1, obj2);
            if (diff == string.Empty)
                return;

            var filePath = Path.Combine(OutPath, $"{name}.txt");
            File.WriteAllText(filePath, diff);
            Assert.Fail($"Objects are not equal. See {filePath} for details.");
        }

        public static void Init()
        {
            if (!Directory.Exists(TmpPath))
                Directory.CreateDirectory(TmpPath);

            if (!Directory.Exists(OutPath))
                Directory.CreateDirectory(OutPath);
        }

        public static void Cleanup()
        {
            Directory.Delete(TmpPath, true);
        }
    }
}
