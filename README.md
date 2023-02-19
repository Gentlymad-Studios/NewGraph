# NewGraph
![image](https://user-images.githubusercontent.com/530629/219878506-2a12f872-cf5b-468e-8982-066c742bb8e7.png)
A general node graph solution centered on data management. This is based on the idea to visualize complex data structures as graph networks without having to modify already established data classes, except adding [Node], [Port] and [SerializeReference] attributes to all classes that should show in the Graph View. 

This project requires:
1. https://github.com/Gentlymad-Studios/GraphViewBase
2. OdinSerializer (not inspector) https://github.com/Gentlymad-Studios/OdinSerializer (Original OdinSerializer repository: https://github.com/TeamSirenix/odin-serializer)

## Installation

### Option 1: Using our Package Manager Tools (and automatically resolve git dependencies)
1. Open your Package Manager window in Unity
2. Click on the '+' icon and select 'Add package from git URL...'
3. Add https://github.com/Gentlymad-Studios/PackageManagerTools.git to install our PackageManagerTools
4. Now add this repository: 'Add package from git URL...' > https://github.com/Gentlymad-Studios/NewGraph.git
5. Wait a few moments until PackageManagerTools picked up all dependencies.
6. You are ready to go.

### Option 2: Manual installation
1. Open your Package Manager window in Unity
2. Click on the '+' icon and select 'Add package from git URL...'
3. Add https://github.com/Gentlymad-Studios/OdinSerializer.git package.
4. Add https://github.com/Gentlymad-Studios/GraphViewBase.git package.
5. Add https://github.com/Gentlymad-Studios/NewGraph.git package.
6. You are ready to go.

## Usage
1. Go to Tools/GraphWindow to Open the graph window. This is your hub where all graphs are created.
2. Click on the "Create" button to create your first graph.

## Creating new Nodes
Creating new Nodes is done by adding certain Attributes (```[Node]```,```[Output]```,```[GraphDisplay]```,```[PortList]```) and implement the ```INode``` interface in a serializable class.

Here is a simple example:
```c#
// Make sure your class is serializable
// And add the Node attribute. Here you can define a spcial color for your node as well as organize it with subcategories.
[Serializable, Node("#007F00FF", "Special")]
public class AnotherTestNode : INode { // Make sure to implement the INode interface so the graph can serialize this easily.
    // with the output attribute you can create visual connectable ports in the graph view to connect to other nodes.
    // it is important to also add the SerializeReference attribute, so that unity serializes this field as a reference.
    [Output, SerializeReference]
    public TestNode output = null;
    
    // With the PortList attribute you can instruct the graph view to visualize a dynamic list of ports. 
    [PortList, SerializeReference]
    public List<TestNode> dataList;

    [Serializable]
    public class SpecialData {
        [Output, SerializeReference] // You can assign outputs and all other attributes in nested classes!
        public TestNode anotherOutput = null;
        // Per default, all data will show up automatically in the side inspector in the GraphView.
        // if you wish to display something directly on the node use the GraphDisplay attribute.
        [GraphDisplay(DisplayType.BothViews)] // DisplayType.BothViews visualizes this field both in the inspector and the node itself. 
        public bool toggle;
    }
    public SpecialData specialData;
}
```
After adding the required attributes and implementing the interface you node should be immidiatley available in the GraphView.

## Notes
1. You can also create editor only utility nodes. See the source code for the CommentNode (https://github.com/Gentlymad-Studios/NewGraph/blob/main/Editor/Views/Nodes/CommentNode.cs) to learn more about it. 
2. Graph assets are simple Scriptable Objects and everything will be fully serialized. In our pipeline we will still convert this representations to the specific use case. In theory you could use a graph asset directly on runtime.
3. The Graph Editor is intended for editor usage ONLY. We are operating with Unity's serialization system under the hood.
