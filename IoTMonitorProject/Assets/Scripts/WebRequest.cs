using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts
{
    internal class WebRequest : MonoBehaviour
    {

        UnityWebRequest request;
        string bodyJsonString;

        private void Start()
        {
            UnityWebRequest req = UnityWebRequest.PostWwwForm("https://a.klaviyo.com/api/v2/list/YeXzKp/members?api_key=pk_ec448e1bdab7504c143466ca5eeabc5e95&profiles=[]", bodyJsonString);
        }








    }
}
