﻿namespace OrderCloud.Integrations.Vertex.Models
{
    public class VertexTokenResponse
    {
        public string access_token { get; set; }

        public int expires_in { get; set; }

        public string token_type { get; set; }
    }
}
