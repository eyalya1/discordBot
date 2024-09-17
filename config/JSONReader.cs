
using Newtonsoft.Json;

namespace discordBot.config
{
    internal class JSONReader
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public async Task ReadJSON()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = await r.ReadToEndAsync();
                JSONSturcture data = JsonConvert.DeserializeObject<JSONSturcture>(json);
                this.token = data.token;
                this.prefix = data.prefix;
            }
        }
    }
    internal sealed class JSONSturcture
    {
        public string token { get; set; }
        public string prefix { get; set; }
    }
}
