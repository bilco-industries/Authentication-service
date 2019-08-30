using MyCouch;
using MyCouch.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System;

namespace Authentication.Model
{
    public class User
    {
        /* INITITIALISE */
        #region Initilisations
        public string DocId { get; internal set; }
        public string Username { get; internal set; }
        internal Password Password { get; set; }
        public string Lastlogin { get; internal set; }
        public string Created { get; internal set; }
        public bool Admin { get; internal set; }
        public bool Authenticated { get; internal set; }
        #endregion
        /* CONSTRUCTORS */
        #region Constructors
        /// <summary>
        /// Object for storing the data about a user
        /// </summary>
        public User() { }

        /// <summary>
        ///  Takes a JObject and creates it into a user object
        /// </summary>
        /// <param name="userJSON">JObject representing the user to be serialised to .net object </param>
        public User(JObject userJSON)
        {
            Username = userJSON.GetValue("username").ToString();
            Password = new Password(userJSON.GetValue("password"));
            Lastlogin = userJSON.TryGetValue("lastLogin", out JToken lastLogin) ? lastLogin.ToString() : string.Empty;
            Created = userJSON.TryGetValue("created", out JToken created) ? created.ToString() : string.Empty;
            Admin = userJSON.GetValue("admin").ToString() == "True" ? true : false;
        }
        #endregion

        /* METHODS */
        #region Methods

        // Converters
        #region Conversions
        /// <summary>
        ///  Convert the curent User object to a JObject
        /// </summary>
        public JObject ToJObject()
        {   
            JObject fullObj = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(this));
            fullObj.AddAfterSelf(this.Password.ToDocObj());
            return fullObj;
        }

        /// <summary>
        /// Convert the current object to a JSON string
        /// </summary>
        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(ToJObject());
        }

        /// <summary>
        ///  Convert the current User object to an object in the format required for couch db storage
        /// </summary>
        /// <returns>Current object without un-needed data for storage</returns>
        public JObject ToDocObj()
        {
            return new JObject()
            {
                { "username", this.Username },
                {
                    "password", new JObject
                    {
                        { "hash", this.Password.Hash},
                        { "salt", this.Password.Salt}
                    }
                },
                { "lastLogin", this.Lastlogin },
                { "created", this.Created },
                { "admin", this.Admin}
            };
        }

        /// <summary>
        /// Convert the doc object to a string
        /// </summary>
        /// <returns> Doc object as a string</returns>
        public string ToDocObjString()
        {
            return JsonConvert.SerializeObject(ToDocObj());
        }
        #endregion

        // Authentication
        #region Authentication
        public bool Authenticate(string password)
        {
            this.Authenticated =  this.Password.CompareHash(password, this.Password);
            return this.Authenticated;
        }
        #endregion  

        // Get
        #region Get
        /// <summary>
        /// Checks if a username is already in use
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if user exists false if not</returns>
        public static async Task<bool> Exists(string username)
        {
            DocumentResponse userDocResponse = await Config.UserDBClient.Documents.GetAsync("user:" + username);
            return !userDocResponse.IsEmpty;
        }

        /// <summary>
        /// gets a user by their username from the couch db 
        /// </summary>
        /// <param name="username">Username of the user to be found</param>
        /// <returns>User object if the user exists otherwise null</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            DocumentResponse userDocResponse = await Config.UserDBClient.Documents.GetAsync("user:" + username);
            if (!userDocResponse.IsEmpty)
            {
                return new User(JsonConvert.DeserializeObject<JObject>(userDocResponse.Content));
            }
            else
            {
                return null;
            }
        }

        public User GetDocUserByUsername(string username)
        {
            DocumentResponse userDocResponse = Config.UserDBClient.Documents.GetAsync("user:" + username).Result;
            if (!userDocResponse.IsEmpty)
            {
                return new User(JsonConvert.DeserializeObject<JObject>(userDocResponse.Content));
            }
            else
            {
                return null;
            }
        }
        #endregion

        // Put 
        #region Put

        /// <summary>
        /// Create a new User object from a password object and a username
        /// </summary>
        /// <param name="username">username of the new user</param>
        /// <param name="password">plaintext password string of the user</param>
        /// <returns>a new user object with the username and password specified</returns>
        public User CreateUser(string username, string password)
        {
            this.Password = new Password(password);
            Username = username;

            return this;
        }

        /// <summary>
        /// Put a user into the couchdb
        /// </summary>
        /// <returns> the success status of the request</returns>
        public async Task<bool> PutUserAsync()
        {
            this.DocId = string.Format("user:{0}", this.Username);
            this.Created = DateTime.UtcNow.ToString(Config.DateFormat);
            DocumentHeaderResponse userDocResponse = await Config.UserDBClient.Documents.PutAsync(this.DocId, this.ToDocObjString());
            return userDocResponse.IsSuccess;
        }
        #endregion
        #endregion
    }
}
