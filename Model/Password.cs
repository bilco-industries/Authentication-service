using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Authentication.Model
{

    public class Password
    {
        internal string Hash { get; set; }
        internal string Salt { get; set; }
        internal string PlainText { get; set; }
        readonly int saltStrength = 20;

        public Password(string hash, string salt)
        {
            this.Hash = hash;
            this.Salt = salt;
        }

        public Password(JObject passwordJObj)
        {
            this.Hash = passwordJObj.GetValue("hash").ToString();
            this.Hash = passwordJObj.GetValue("salt").ToString();
        }

        public Password(JToken passwordJTok)
        {
            try
            {
                JObject passwordJObj = passwordJTok.ToObject<JObject>();
                this.Hash = passwordJObj.GetValue("hash").ToString();
                this.Salt = passwordJObj.GetValue("salt").ToString();
            }
            catch
            {
                this.PlainText = passwordJTok.ToString();
                CreateHash(PlainText);
            }
        }

        internal Password CreateHash(string password)
        {
            this.Salt = BCrypt.Net.BCrypt.GenerateSalt(saltStrength);
            this.Hash = BCrypt.Net.BCrypt.HashPassword(password, this.Salt);

            return this;
        }

        public bool CompareHash(string sentPassword, Password storedPassword)
        {
            string sentHashed = BCrypt.Net.BCrypt.HashPassword(sentPassword, storedPassword.Salt);
            bool valid = string.Compare(storedPassword.Hash, sentHashed) == 0 ? true : false;
            return valid;
        }

        public JObject ToJObject()
        {
            return JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(this));
        }

        public JObject ToDocObj()
        {
            JObject docObj = new JObject()
            {
                { "hash", this.Hash },
                { "Salt", this.Salt }
            };

            return docObj;
        }
    }
}