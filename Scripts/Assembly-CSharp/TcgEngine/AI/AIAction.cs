namespace TcgEngine.AI
{
	public class AIAction
	{
		public ushort type;

		public string card_uid;

		public string target_uid;

		public int target_player_id;

		public string ability_id;

		public Slot slot;

		public int value;

		public int score;

		public int sort;

		public bool valid;

		public static AIAction None => new AIAction
		{
			type = 0
		};

		public AIAction()
		{
		}

		public AIAction(ushort t)
		{
			type = t;
		}

		public string GetText(Game data)
		{
			string text = GameAction.GetString(type);
			Card card = data.GetCard(card_uid);
			Card card2 = data.GetCard(target_uid);
			if (card != null)
			{
				text = text + " card " + card.card_id;
			}
			if (card2 != null)
			{
				text = text + " target " + card2.card_id;
			}
			if (slot != Slot.None)
			{
				text = text + " slot " + slot.x + "-" + slot.p;
			}
			if (ability_id != null)
			{
				text = text + " ability " + ability_id;
			}
			if (value > 0)
			{
				text = text + " value " + value;
			}
			return text;
		}

		public void Clear()
		{
			type = 0;
			valid = false;
			card_uid = null;
			target_uid = null;
			ability_id = null;
			target_player_id = -1;
			slot = Slot.None;
			value = -1;
			score = 0;
			sort = 0;
		}
	}
}
