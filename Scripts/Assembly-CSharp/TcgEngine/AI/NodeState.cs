using System.Collections.Generic;

namespace TcgEngine.AI
{
	public class NodeState
	{
		public int tdepth;

		public int taction;

		public int sort_min;

		public int hvalue;

		public int alpha;

		public int beta;

		public AIAction last_action;

		public int current_player;

		public NodeState parent;

		public NodeState best_child;

		public List<NodeState> childs = new List<NodeState>();

		public NodeState()
		{
		}

		public NodeState(NodeState parent, int player_id, int turn_depth, int turn_action, int turn_sort)
		{
			this.parent = parent;
			current_player = player_id;
			tdepth = turn_depth;
			taction = turn_action;
			sort_min = turn_sort;
		}

		public void Clear()
		{
			last_action = null;
			best_child = null;
			parent = null;
			childs.Clear();
		}
	}
}
