using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deseralizer
{
    string t = "NsTeST  4 nabijamrepe9 2 31135 79.110.84.132:4016:1:1.7.Feniks 79.110.84.132:4014:1:1.5.Feniks 79.110.84.132:4015:0:1.6.Feniks 79.110.84.132:4011:7:1.2.Feniks 79.110.84.132:4012:1:1.3.Feniks 79.110.84.132:4013:1:1.4.Feniks 79.110.84.132:4010:1:1.1.Feniks -1:-1:-1:10000.10000.1";

    void NsTeST(string packet)
    {

        var dictionary = new Dictionary<string, int>();
        dictionary.Add("cat", 2);
        dictionary.Add("dog", 1);
        dictionary.Add("llama", 0);
        
    }
}
