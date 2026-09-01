namespace SoccerGame.Core
{
    public enum TeamSide { Home, Away, None }

    public enum PositionRole { GK, LB, LCB, CB, RCB, RB, LM, LCM, CM, RCM, RM, LW, LF, CF, RF, RW, ST }

    public enum GamePhase { Attack, Defense, TransitionToAttack, TransitionToDefense, SetPiece }

    public enum MentalityType { VeryDefensive, Defensive, Balanced, Attacking, VeryAttacking }

    public enum BallState { Rolling, Flying, Dead, InPossession }

    public enum SetPieceType { Kickoff, FreeKick, CornerKick, ThrowIn, GoalKick, PenaltyKick, DropBall, None }
}
