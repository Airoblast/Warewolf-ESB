namespace Dev2.Common.Interfaces.UndoFramework
{
    public class SimpleHistoryNode
    {
        public SimpleHistoryNode()
        {
        }

        public SimpleHistoryNode(IAction lastExistingAction, SimpleHistoryNode lastExistingState)
        {
            PreviousAction = lastExistingAction;
            PreviousNode = lastExistingState;
        }

        public IAction NextAction { get; set; }

        public SimpleHistoryNode NextNode { get; set; }

        public IAction PreviousAction { get; set; }

        public SimpleHistoryNode PreviousNode { get; set; }
    }
}

