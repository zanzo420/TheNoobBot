if(!MovementManager.InMovement)
{
	if (questObjective.Position.IsValid && questObjective.Position.DistanceTo(ObjectManager.Me.Position) > 5f)
	{ 
		Logging.Write("enter objectif 1");
		MountTask.Mount();
		System.Threading.Thread.Sleep(2000);
		var listP = new List<Point>();
		listP.Add(ObjectManager.Me.Position);
		listP.Add(questObjective.Position);
		MovementManager.Go(listP);
		while(MovementManager.InMovement && questObjective.Position.DistanceTo(ObjectManager.Me.Position) > 5f)
		{
		    System.Threading.Thread.Sleep(100);
		}
		MovementManager.StopMove();
	}
	if (questObjective.Position.DistanceTo(ObjectManager.Me.Position) <= 5f)
	{
		Logging.Write("Completed");
        questObjective.IsObjectiveCompleted = true; 
		return true;
    }
}