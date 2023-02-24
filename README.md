# NewGraph
![image](https://user-images.githubusercontent.com/530629/219878506-2a12f872-cf5b-468e-8982-066c742bb8e7.png)
A data oriented node graph solution for **Unity** powered by **UIToolkit (UIElements)**. This is based on the idea to visualize complex data structures as graph networks without having to modify already established data classes, except adding `[Node]`, `[Port]`, `[PortList]` and `[SerializeReference]` attributes to all classes that should show in the Graph View.

### Features:
* `WYSIWYG.` A port connecting to a node in the graph is a `real reference` to the object in your `graph .asset`
* Good performance even with many nodes as this is built upon  the new retained `UIToolkit` UI system.
* build visual data oriented networks based on your custom `[Serializable]` data classes simply by `adding attributes`.
* `Fully serialized dynamic lists` of ports with the [`[PortList]`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage#portlist) attribute.
* Customize data visualization with [`[GraphDisplay]`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage#graphdisplay): Display a field directly inside the graph? The side inspector? In both views?
* Full support of Unity's native `Undo/Redo` stack.
* Support for `Utility Nodes` that can be created to help organize your specific graph business.
* `Copy & Paste` even across graphs.
* No child `ScriptableObject` mayhem! A graph asset will hold all your data in `one single scriptable object`.
* `Cyclic references` and `complex reference chains` are natively supported with the use of [SerializeReference] attribute.
* `No reliance` on the now deprecated `Experimental.GraphView`. Everything was written from scratch or is based on maintainable code.
* `Keyboard shortcuts`, a `searchable context menu`, default UtilityNodes like [`CommentNode`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage#groupcommentnode), [`GroupCommentNode`](https://github.com/Gentlymad-Studios/NewGraph/wiki/2.-Usage#groupcommentnode), a extensively commented code base and more...

See the [Wiki](https://github.com/Gentlymad-Studios/NewGraph/wiki) for installation & usage instructions.
