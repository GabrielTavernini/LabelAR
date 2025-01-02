using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class Coordinates
{
    public float east;
    public float north;
    public float altitude;
}

public class Label
{
    public float x;
    public float y;
    public float z;
    public float distance;
    public string name;
    public List<string> buildings;
}

public class Response
{
    public Coordinates coordinates;
    public List<Label> labels;
    public HashSet<string> buildings;
    public float visibility;
    public List<string> alignment_labels;
}

public class AddLabelPayload
{
    public string name;
    public float north;
    public float east;
    public float height;
    public List<string> buildings;
}

public class EditLabelPayload {
    public string oldName;
    public string newName;
}

public class DeleteLabelPayload {
    public string name;
}

public class Request
{
    public static readonly Dictionary<string, string> spaces = new Dictionary<string, string>{
        { "Polyterrasse", "{\"coordinates\":{\"east\":2683708.8322689505,\"north\":1247844.379374196,\"altitude\":451.7},\"labels\":[{\"name\":\"HG\",\"distance\":18.0,\"x\":-4.674615619238466,\"y\":23.30000000000001,\"z\":17.38621420180425,\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\"]},{\"name\":\"UZH\",\"distance\":176.84,\"x\":90.65384592860937,\"y\":48.30000000000001,\"z\":-151.8418320545461,\"buildings\":[\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\"]},{\"name\":\"Predigerkirche\",\"distance\":285.08,\"x\":-144.81379565270618,\"y\":28.30000000000001,\"z\":-245.57527858647518,\"buildings\":[\"1E48EB\",\"2FEDF5\"]},{\"name\":\"Z\xFCrich Hauptbahnhof\",\"distance\":477.57,\"x\":-448.33592081209645,\"y\":-43.69999999999999,\"z\":164.57971798139624,\"buildings\":[\"1155B8\",\"192709\",\"1468D5\"]},{\"name\":\"Observatory\",\"distance\":612.46,\"x\":-575.5816512759775,\"y\":15.915999999999997,\"z\":-209.37823552940972,\"buildings\":[\"15D5C5\"]},{\"name\":\"Dan\",\"distance\":697.35,\"x\":-430.8316510617733,\"y\":42.48033000000004,\"z\":-548.3782356895972,\"buildings\":[]},{\"name\":\"Grossm\xFCnster\",\"distance\":704.58,\"x\":-231.96469120960683,\"y\":18.30000000000001,\"z\":-665.3250149637461,\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\"]},{\"name\":\"Sankt Peeter\",\"distance\":718.17,\"x\":-451.36364175146446,\"y\":23.30000000000001,\"z\":-558.6393219237216,\"buildings\":[\"3241D9\",\"CB8AB\"]},{\"name\":\"Fraum\xFCnster\",\"distance\":826.59,\"x\":-408.5060771666467,\"y\":18.30000000000001,\"z\":-718.6241851064842,\"buildings\":[\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"]},{\"name\":\"Lake Zurich\",\"distance\":1428.59,\"x\":-303.71802193904296,\"y\":-45.69999999999999,\"z\":-1395.9810485781636,\"buildings\":[]},{\"name\":\"Z\xFCrichberg\",\"distance\":2243.9,\"x\":1508.1534001994878,\"y\":238.3,\"z\":1661.5916680733208,\"buildings\":[]},{\"name\":\"B1\",\"distance\":2815.52,\"x\":78.1567293270491,\"y\":38.30000000000001,\"z\":2814.539540325757,\"buildings\":[\"2CCEF\"]},{\"name\":\"B2\",\"distance\":2866.91,\"x\":60.918348263483495,\"y\":42.92349999999999,\"z\":2866.371765232878,\"buildings\":[\"9D3DD\"]},{\"name\":\"B3\",\"distance\":2910.06,\"x\":6.6855280278250575,\"y\":38.30000000000001,\"z\":2910.156625482021,\"buildings\":[\"201C2D\"]},{\"name\":\"Kirche Oerlikon\",\"distance\":3351.3,\"x\":-98.51863316586241,\"y\":43.30000000000001,\"z\":3349.977914801333,\"buildings\":[\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\"]},{\"name\":\"Oerlikon Velodrome\",\"distance\":3735.13,\"x\":261.98494131071493,\"y\":-11.699999999999989,\"z\":3726.0760916077998,\"buildings\":[\"14878\",\"966E2\",\"28956\",\"2BA9\"]},{\"name\":\"Hallenstadion\",\"distance\":3941.0,\"x\":294.6606561313383,\"y\":8.300000000000011,\"z\":3930.117584093008,\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"]},{\"name\":\"Fernsehturm Uetliberg\",\"distance\":5139.79,\"x\":-4163.899940898176,\"y\":548.3,\"z\":-3013.519411721034,\"buildings\":[]},{\"name\":\"Hörnli\",\"distance\":31818.88,\"x\":31318.438989240676,\"y\":698.3,\"z\":-5626.863490870688,\"buildings\":[]},{\"name\":\"Tödi\",\"distance\":68731.9,\"x\":28948.758633526508,\"y\":3188.3,\"z\":-62339.31956329732,\"buildings\":[]}],\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\",\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\",\"1E48EB\",\"2FEDF5\",\"1155B8\",\"192709\",\"1468D5\",\"15D5C5\",\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\",\"3241D9\",\"CB8AB\",\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\",\"2CCEF\",\"9D3DD\",\"201C2D\",\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\",\"14878\",\"966E2\",\"28956\",\"2BA9\",\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"],\"visibility\":39820.0,\"alignment_labels\":[\"8E003\",\"15D5C5\",\"2FEDF5\"]}" },
        { "Andreasturm", "{\"coordinates\":{\"east\":2683678.5288211247,\"north\":1251941.3266481836,\"altitude\":493.0},\"labels\":[{\"name\":\"Hallenstadion\",\"distance\":365.27,\"x\":324.9641039571725,\"y\":-33.0,\"z\":-166.82968989457004,\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"]},{\"name\":\"Oerlikon Velodrome\",\"distance\":472.19,\"x\":292.2883891365491,\"y\":-53.0,\"z\":-370.8711823797785,\"buildings\":[\"14878\",\"966E2\",\"28956\",\"2BA9\"]},{\"name\":\"Kirche Oerlikon\",\"distance\":750.05,\"x\":-68.21518534002826,\"y\":2.0,\"z\":-746.9693591862451,\"buildings\":[\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\"]},{\"name\":\"B3\",\"distance\":1187.32,\"x\":36.98897585365921,\"y\":-3.0,\"z\":-1186.7906485055573,\"buildings\":[\"201C2D\"]},{\"name\":\"B2\",\"distance\":1233.9,\"x\":91.22179608931765,\"y\":1.6234999999999786,\"z\":-1230.5755087547004,\"buildings\":[\"9D3DD\"]},{\"name\":\"B1\",\"distance\":1286.93,\"x\":108.46017715288326,\"y\":-3.0,\"z\":-1282.407733661821,\"buildings\":[\"2CCEF\"]},{\"name\":\"Z\xFCrichberg\",\"distance\":2880.48,\"x\":1538.456848025322,\"y\":197.0,\"z\":-2435.3556059142575,\"buildings\":[]},{\"name\":\"Z\xFCrich Hauptbahnhof\",\"distance\":3954.38,\"x\":-418.0324729862623,\"y\":-85.0,\"z\":-3932.367556006182,\"buildings\":[\"1155B8\",\"192709\",\"1468D5\"]},{\"name\":\"HG\",\"distance\":4079.49,\"x\":25.62883220659569,\"y\":-18.0,\"z\":-4079.561059785774,\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\"]},{\"name\":\"UZH\",\"distance\":4250.35,\"x\":120.95729375444353,\"y\":7.0,\"z\":-4248.789106042124,\"buildings\":[\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\"]},{\"name\":\"Observatory\",\"distance\":4340.55,\"x\":-545.2782034501433,\"y\":-25.384000000000015,\"z\":-4306.325509516988,\"buildings\":[\"15D5C5\"]},{\"name\":\"Predigerkirche\",\"distance\":4343.87,\"x\":-114.51034782687202,\"y\":-13.0,\"z\":-4342.522552574053,\"buildings\":[\"1E48EB\",\"2FEDF5\"]},{\"name\":\"Dan\",\"distance\":4662.38,\"x\":-400.52820323593915,\"y\":1.1803300000000263,\"z\":-4645.325509677175,\"buildings\":[]},{\"name\":\"Sankt Peeter\",\"distance\":4674.41,\"x\":-421.0601939256303,\"y\":-18.0,\"z\":-4655.5865959113,\"buildings\":[\"3241D9\",\"CB8AB\"]},{\"name\":\"Grossm\xFCnster\",\"distance\":4766.36,\"x\":-201.66124338377267,\"y\":-23.0,\"z\":-4762.272288951324,\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\"]},{\"name\":\"Fraum\xFCnster\",\"distance\":4830.22,\"x\":-378.20262934081256,\"y\":-23.0,\"z\":-4815.571459094062,\"buildings\":[\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"]},{\"name\":\"Lake Zurich\",\"distance\":5499.53,\"x\":-273.4145741132088,\"y\":-87.0,\"z\":-5492.928322565742,\"buildings\":[]},{\"name\":\"Fernsehturm Uetliberg\",\"distance\":8224.37,\"x\":-4133.596493072342,\"y\":507.0,\"z\":-7110.466685708612,\"buildings\":[]},{\"name\":\"Hörnli\",\"distance\":32821.05,\"x\":31348.74243706651,\"y\":657.0,\"z\":-9723.810764858266,\"buildings\":[]},{\"name\":\"Tödi\",\"distance\":72480.24,\"x\":28979.062081352342,\"y\":3147.0,\"z\":-66436.2668372849,\"buildings\":[]}],\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\",\"14878\",\"966E2\",\"28956\",\"2BA9\",\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\",\"201C2D\",\"9D3DD\",\"2CCEF\",\"1155B8\",\"192709\",\"1468D5\",\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\",\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\",\"15D5C5\",\"1E48EB\",\"2FEDF5\",\"3241D9\",\"CB8AB\",\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\",\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"],\"visibility\":36000.0,\"alignment_labels\":[\"E7E7\",\"E63CB\",\"82F35\"]}" },
        { "Bellevue", "{\"coordinates\":{\"east\":2683580.036023246,\"north\":1246851.5010302756,\"altitude\":412.0},\"labels\":[{\"name\":\"Grossm\xFCnster\",\"distance\":343.41,\"x\":-103.16844550520182,\"y\":58.0,\"z\":327.5533289567102,\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\"]},{\"name\":\"Fraum\xFCnster\",\"distance\":391.72,\"x\":-279.7098314622417,\"y\":58.0,\"z\":274.254158813972,\"buildings\":[\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"]},{\"name\":\"Lake Zurich\",\"distance\":439.4,\"x\":-174.92177623463795,\"y\":-6.0,\"z\":-403.10270465770736,\"buildings\":[]},{\"name\":\"Dan\",\"distance\":537.39,\"x\":-302.0354053573683,\"y\":82.18033000000003,\"z\":444.50010823085904,\"buildings\":[]},{\"name\":\"Sankt Peeter\",\"distance\":540.92,\"x\":-322.56739604705945,\"y\":63.0,\"z\":434.23902199673466,\"buildings\":[\"3241D9\",\"CB8AB\"]},{\"name\":\"Predigerkirche\",\"distance\":747.45,\"x\":-16.017549948301166,\"y\":68.0,\"z\":747.3030653339811,\"buildings\":[\"1E48EB\",\"2FEDF5\"]},{\"name\":\"UZH\",\"distance\":869.17,\"x\":219.45009163301438,\"y\":88.0,\"z\":841.0365118659101,\"buildings\":[\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\"]},{\"name\":\"Observatory\",\"distance\":901.91,\"x\":-446.7854055715725,\"y\":55.615999999999985,\"z\":783.5001083910465,\"buildings\":[\"15D5C5\"]},{\"name\":\"HG\",\"distance\":1017.83,\"x\":124.12163008516654,\"y\":63.0,\"z\":1010.2645581222605,\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\"]},{\"name\":\"Z\xFCrich Hauptbahnhof\",\"distance\":1200.71,\"x\":-319.53967510769144,\"y\":-4.0,\"z\":1157.4580619018525,\"buildings\":[\"1155B8\",\"192709\",\"1468D5\"]},{\"name\":\"Z\xFCrichberg\",\"distance\":3118.52,\"x\":1636.9496459038928,\"y\":278.0,\"z\":2654.470011993777,\"buildings\":[]},{\"name\":\"B1\",\"distance\":3812.9,\"x\":206.95297503145412,\"y\":78.0,\"z\":3807.4178842462134,\"buildings\":[\"2CCEF\"]},{\"name\":\"B2\",\"distance\":3863.77,\"x\":189.7145939678885,\"y\":82.62349999999998,\"z\":3859.250109153334,\"buildings\":[\"9D3DD\"]},{\"name\":\"B3\",\"distance\":3905.24,\"x\":135.48177373223007,\"y\":78.0,\"z\":3903.034969402477,\"buildings\":[\"201C2D\"]},{\"name\":\"Kirche Oerlikon\",\"distance\":4342.8,\"x\":30.2776125385426,\"y\":83.0,\"z\":4342.856258721789,\"buildings\":[\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\"]},{\"name\":\"Fernsehturm Uetliberg\",\"distance\":4512.6,\"x\":-4035.1036951937713,\"y\":588.0,\"z\":-2020.6410678005777,\"buildings\":[]},{\"name\":\"Oerlikon Velodrome\",\"distance\":4734.93,\"x\":390.78118701511994,\"y\":28.0,\"z\":4718.954435528256,\"buildings\":[\"14878\",\"966E2\",\"28956\",\"2BA9\"]},{\"name\":\"Hallenstadion\",\"distance\":4940.99,\"x\":423.45690183574334,\"y\":48.0,\"z\":4922.995928013464,\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"]},{\"name\":\"Hörnli\",\"distance\":31785.82,\"x\":31447.23523494508,\"y\":738.0,\"z\":-4633.985146950232,\"buildings\":[]},{\"name\":\"Tödi\",\"distance\":67887.77,\"x\":29077.554879230913,\"y\":3228.0,\"z\":-61346.441219376866,\"buildings\":[]}],\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\",\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\",\"3241D9\",\"CB8AB\",\"1E48EB\",\"2FEDF5\",\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\",\"15D5C5\",\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\",\"1155B8\",\"192709\",\"1468D5\",\"2CCEF\",\"9D3DD\",\"201C2D\",\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\",\"14878\",\"966E2\",\"28956\",\"2BA9\",\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"],\"visibility\":39920.0,\"alignment_labels\":[\"176C66\",\"183530 \",\"8A55\"]}" },
        { "WG7", "{\"coordinates\":{\"east\":2683739.410585882,\"north\":1250631.804456769,\"altitude\":475.0},\"labels\":[{\"name\":\"B1\",\"distance\":54.76,\"x\":47.57841239543632,\"y\":15.0,\"z\":27.11445775278844,\"buildings\":[\"2CCEF\"]},{\"name\":\"B2\",\"distance\":84.57,\"x\":30.340031331870705,\"y\":19.62349999999998,\"z\":78.94668265990913,\"buildings\":[\"9D3DD\"]},{\"name\":\"B3\",\"distance\":125.03,\"x\":-23.892788903787732,\"y\":15.0,\"z\":122.73154290905222,\"buildings\":[\"201C2D\"]},{\"name\":\"Kirche Oerlikon\",\"distance\":577.15,\"x\":-129.0969500974752,\"y\":20.0,\"z\":562.5528322283644,\"buildings\":[\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\"]},{\"name\":\"Oerlikon Velodrome\",\"distance\":966.72,\"x\":231.40662437910214,\"y\":-35.0,\"z\":938.6510090348311,\"buildings\":[\"14878\",\"966E2\",\"28956\",\"2BA9\"]},{\"name\":\"Hallenstadion\",\"distance\":1172.76,\"x\":264.08233919972554,\"y\":-15.0,\"z\":1142.6925015200395,\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"]},{\"name\":\"Z\xFCrichberg\",\"distance\":1857.54,\"x\":1477.575083267875,\"y\":215.0,\"z\":-1125.833414499648,\"buildings\":[]},{\"name\":\"Z\xFCrich Hauptbahnhof\",\"distance\":2666.11,\"x\":-478.91423774370924,\"y\":-67.0,\"z\":-2622.8453645915724,\"buildings\":[\"1155B8\",\"192709\",\"1468D5\"]},{\"name\":\"HG\",\"distance\":2770.16,\"x\":-35.252932550851256,\"y\":0.0,\"z\":-2770.0388683711644,\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\"]},{\"name\":\"UZH\",\"distance\":2939.77,\"x\":60.07552899699658,\"y\":25.0,\"z\":-2939.266914627515,\"buildings\":[\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\"]},{\"name\":\"Predigerkirche\",\"distance\":3037.95,\"x\":-175.39211258431897,\"y\":5.0,\"z\":-3033.000361159444,\"buildings\":[\"1E48EB\",\"2FEDF5\"]},{\"name\":\"Observatory\",\"distance\":3057.38,\"x\":-606.1599682075903,\"y\":-7.3840000000000146,\"z\":-2996.8033181023784,\"buildings\":[\"15D5C5\"]},{\"name\":\"Dan\",\"distance\":3367.44,\"x\":-461.4099679933861,\"y\":19.180330000000026,\"z\":-3335.803318262566,\"buildings\":[]},{\"name\":\"Sankt Peeter\",\"distance\":3380.47,\"x\":-481.94195868307725,\"y\":0.0,\"z\":-3346.0644044966903,\"buildings\":[\"3241D9\",\"CB8AB\"]},{\"name\":\"Grossm\xFCnster\",\"distance\":3462.59,\"x\":-262.5430081412196,\"y\":-5.0,\"z\":-3452.7500975367147,\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\"]},{\"name\":\"Fraum\xFCnster\",\"distance\":3533.31,\"x\":-439.0843940982595,\"y\":-5.0,\"z\":-3506.049267679453,\"buildings\":[\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"]},{\"name\":\"Lake Zurich\",\"distance\":4196.59,\"x\":-334.29633887065575,\"y\":-69.0,\"z\":-4183.406131151132,\"buildings\":[]},{\"name\":\"Fernsehturm Uetliberg\",\"distance\":7158.27,\"x\":-4194.478257829789,\"y\":525.0,\"z\":-5800.944494294003,\"buildings\":[]},{\"name\":\"Hörnli\",\"distance\":32398.45,\"x\":31287.860672309063,\"y\":675.0,\"z\":-8414.288573443657,\"buildings\":[]},{\"name\":\"Tödi\",\"distance\":71257.18,\"x\":28918.180316594895,\"y\":3165.0,\"z\":-65126.74464587029,\"buildings\":[]}],\"buildings\":[\"2CCEF\",\"9D3DD\",\"201C2D\",\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\",\"14878\",\"966E2\",\"28956\",\"2BA9\",\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\",\"1155B8\",\"192709\",\"1468D5\",\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\",\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\",\"1E48EB\",\"2FEDF5\",\"15D5C5\",\"3241D9\",\"CB8AB\",\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\",\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"],\"visibility\":34380.0,\"alignment_labels\":[\"2CCEF\",\"9D3DD\",\"201C2D\"]}" },
        { "Lindenhof", "{\"coordinates\":{\"east\":2683284.5282953605,\"north\":1247492.0262800264,\"altitude\":429.0},\"labels\":[{\"name\":\"Sankt Peeter\",\"distance\":208.04,\"x\":-27.05966816144064,\"y\":54.0,\"z\":-206.2862277540844,\"buildings\":[\"3241D9\",\"CB8AB\"]},{\"name\":\"Observatory\",\"distance\":208.14,\"x\":-151.27767768595368,\"y\":46.615999999999985,\"z\":142.97485864022747,\"buildings\":[\"15D5C5\"]},{\"name\":\"Predigerkirche\",\"distance\":299.18,\"x\":279.49017793731764,\"y\":59.0,\"z\":106.77781558316201,\"buildings\":[\"1E48EB\",\"2FEDF5\"]},{\"name\":\"Fraumünster\",\"distance\":366.6,\"x\":15.797896423377097,\"y\":49.0,\"z\":-366.27109093684703,\"buildings\":[\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\"]},{\"name\":\"Grossmünster\",\"distance\":367.34,\"x\":192.339282380417,\"y\":49.0,\"z\":-312.9719207941089,\"buildings\":[\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\"]},{\"name\":\"Zürich Hauptbahnhof\",\"distance\":517.47,\"x\":-24.03194722207263,\"y\":-13.0,\"z\":516.9328121510334,\"buildings\":[\"1155B8\",\"192709\",\"1468D5\"]},{\"name\":\"UZH\",\"distance\":552.6,\"x\":514.9578195186332,\"y\":79.0,\"z\":200.51126211509109,\"buildings\":[\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\"]},{\"name\":\"HG\",\"distance\":559.26,\"x\":419.62935797078535,\"y\":54.0,\"z\":369.73930837144144,\"buildings\":[\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\"]},{\"name\":\"Lake Zurich\",\"distance\":1050.54,\"x\":120.58595165098086,\"y\":-15.0,\"z\":-1043.6279544085264,\"buildings\":[]},{\"name\":\"Zürichberg\",\"distance\":2791.03,\"x\":1932.4573737895116,\"y\":269.0,\"z\":2013.944762242958,\"buildings\":[]},{\"name\":\"B1\",\"distance\":3206.39,\"x\":502.4607029170729,\"y\":69.0,\"z\":3166.8926344953943,\"buildings\":[\"2CCEF\"]},{\"name\":\"B2\",\"distance\":3254.97,\"x\":485.2223218535073,\"y\":73.62349999999998,\"z\":3218.724859402515,\"buildings\":[\"9D3DD\"]},{\"name\":\"B3\",\"distance\":3290.73,\"x\":430.9895016178489,\"y\":69.0,\"z\":3262.509719651658,\"buildings\":[\"201C2D\"]},{\"name\":\"Kirche Oerlikon\",\"distance\":3716.5,\"x\":325.7853404241614,\"y\":74.0,\"z\":3702.3310089709703,\"buildings\":[\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\"]},{\"name\":\"Oerlikon Velodrome\",\"distance\":4135.61,\"x\":686.2889149007387,\"y\":19.0,\"z\":4078.429185777437,\"buildings\":[\"14878\",\"966E2\",\"28956\",\"2BA9\"]},{\"name\":\"Hallenstadion\",\"distance\":4342.24,\"x\":718.9646297213621,\"y\":39.0,\"z\":4282.470678262645,\"buildings\":[\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"]},{\"name\":\"Fernsehturm Uetliberg\",\"distance\":4589.64,\"x\":-3739.5959673081525,\"y\":579.0,\"z\":-2661.166317551397,\"buildings\":[]},{\"name\":\"Hörnli\",\"distance\":32176.95,\"x\":31742.7429628307,\"y\":729.0,\"z\":-5274.510396701051,\"buildings\":[]},{\"name\":\"Tödi\",\"distance\":68593.11,\"x\":29373.06260711653,\"y\":3219.0,\"z\":-61986.966469127685,\"buildings\":[]}],\"buildings\":[\"3241D9\",\"CB8AB\",\"15D5C5\",\"1E48EB\",\"2FEDF5\",\"12712D\",\"1379C2\",\"13AD47\",\"2907FE\",\"308FD5\",\"10A18D\",\"21D5AB\",\"2417E3\",\"279536\",\"2E1B86\",\"2E4719\",\"30BC1B\",\"1155B8\",\"192709\",\"1468D5\",\"2CABEF\",\"1FB17D\",\"4B92C\",\"7AF27\",\"5BEA1\",\"E80A8\",\"113008\",\"16F29E\",\"2110F3\",\"228394\",\"2A5AC7\",\"2CCEF\",\"9D3DD\",\"201C2D\",\"1620A\",\"16A3B\",\"19EA4\",\"1D4ED\",\"5385E\",\"BB979\",\"C6F9A\",\"14878\",\"966E2\",\"28956\",\"2BA9\",\"34E2B\",\"63A90\",\"753D7\",\"AADA6\",\"ABD35\",\"CB347\"],\"visibility\":63980.0,\"alignment_labels\":[\"2DD6D0\",\"2FEDF5\",\"5BEA1\"]}" },
    };
    public static Response response;
    private static readonly string baseUrl = "labelar.ilbrigante.me";

    private Request() { }

    public static IEnumerator Load(string mapName)
    {
        if(Orchestrator.DEMO) {
            Debug.Log("DEMO MODE: Skipping get_labels request");
            response = JsonConvert.DeserializeObject<Response>(spaces.GetValueOrDefault(mapName, "{}"));
            yield break;
        }

        string url = $"{baseUrl}/get_labels?mapName={mapName}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
                response = null;
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Labels received from server: " + jsonResponse);
                response = JsonConvert.DeserializeObject<Response>(jsonResponse);
            }
        }
    }



    public static IEnumerator AddLabel(AddLabelPayload payload)
    {
        if(Orchestrator.DEMO) {
            Debug.Log("DEMO MODE: Skipping add_label request");
            yield break;
        }

        string url = $"{baseUrl}/add_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Add label response: " + jsonResponse);
            }
        }
    }

    public static IEnumerator EditLabel(EditLabelPayload payload)
    {
        if(Orchestrator.DEMO) {
            Debug.Log("DEMO MODE: Skipping edit_label request");
            yield break;
        }

        Debug.Log("Edit: " + payload.oldName);
        string url = $"{baseUrl}/edit_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Edit label response: " + jsonResponse);
            }
        }
    }

    public static IEnumerator DeleteLabel(DeleteLabelPayload payload)
    {
        if(Orchestrator.DEMO) {
            Debug.Log("DEMO MODE: Skipping delete_label request");
            yield break;
        }

        Debug.Log("Delete: " + payload.name);
        string url = $"{baseUrl}/delete_label";
        using (UnityWebRequest request = UnityWebRequest.Post(url, JsonConvert.SerializeObject(payload), "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching data: " + request.error);
            }
            else
            {
                // Parse the JSON response
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Delete label response: " + jsonResponse);
            }
        }
    }
}