using System;
using System.Collections.Generic;

[Serializable]
public class NodeData
{
    public string id;
    public int group;
    public List<string> entities;
}

[Serializable]
public class LinkData
{
    public string source;
    public string target;
    public int weight;
}

[Serializable]
public class GraphData
{
    public List<NodeData> nodes;
    public List<LinkData> links;
}
