# NewGraph
![image](https://user-images.githubusercontent.com/530629/219878506-2a12f872-cf5b-468e-8982-066c742bb8e7.png)
Our general node graph solution. This is build upon the idea to visualize complex data structures as graph networks without having to modify already established data classes, except adding [Node], [Port] and [SerializeReference] attributes to all classes that should show in the Graph View. 

This project requires:  https://github.com/Gentlymad-Studios/GraphViewBase and OdinSerializer (not inspector) https://github.com/Gentlymad-Studios/OdinSerializer to work. (Original OdinSerializer repository: https://github.com/TeamSirenix/odin-serializer)

Added INode and these attributes...
![image](https://user-images.githubusercontent.com/530629/219878624-4eb1f569-f0f8-45d0-b9a0-9915c7109de8.png)
...will result in this node in the graph
![image](https://user-images.githubusercontent.com/530629/219878644-5a1d0cba-7c56-477b-9bdd-afa7207478ce.png)
