using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MiniJam159.AI;

namespace MiniJam159.AI
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        BUILD,
        HARVEST
    }

    public class Command
    {
        public CommandType commandType;
        public string tooltip = "DEFAULT COMMAND TOOLTIP";

        public virtual void initialize() { }

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }
    }
    public class MoveCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Move</b>\nMoves the selected units to target location";
        }

        public override void execute()
        {
            // TODO: Use correct selected list
            List<GameAI> selectedAIs = new List<GameAI>();
            foreach (var ai in selectedAIs)
            {
                var type = ai.GetType();
                if (type.GetMethod("moveAICommand") != null)
                {
                    //ai.moveAICommand(Input.mousePosition);
                }
            }
        }
    }
    public class AttackCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Attack</b>\nAttacks target enemy unit";
        }

        public override void execute()
        {
            // TODO: Use correct selected list
            List<GameAI> selectedAIs = new List<GameAI>();
            foreach (var ai in selectedAIs)
            {
                var type = ai.GetType();
                if (type.GetMethod("attackAICommand") != null)
                {
                    //ai.attackAICommand(Input.mousePosition);
                }
            }
        }
    }
    public class HoldCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Hold</b>\nSelected units will stop moving and attack enemies in range";
        }

        public override void execute()
        {
            // TODO: Use correct selected list
            List<GameAI> selectedAIs = new List<GameAI>();
            foreach (var ai in selectedAIs)
            {
                var type = ai.GetType();
                if (type.GetMethod("holdAICommand") != null)
                {
                    //ai.holdAICommand(Input.mousePosition);
                }
            }
        }
    }
    public class BuildCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute()
        {
            // TODO: Use correct selected list
            List<GameAI> selectedAIs = new List<GameAI>();
            foreach (var ai in selectedAIs)
            {
                var type = ai.GetType();
                if (type.GetMethod("buildAICommand") != null)
                {
                    //ai.buildAICommand(Input.mousePosition);
                }
            }
        }
    }
    public class HarvestCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Harvest</b>\nHarvests the target resource";
        }

        public override void execute()
        {
            // TODO: Use correct selected list
            List<GameAI> selectedAIs = new List<GameAI>();
            foreach (var ai in selectedAIs)
            {
                var type = ai.GetType();
                if (type.GetMethod("harvestAICommand") != null)
                {
                    //ai.harvestAICommand(Input.mousePosition);
                }
            }
        }
    }
}
