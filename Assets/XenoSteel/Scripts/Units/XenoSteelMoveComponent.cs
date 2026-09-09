// using System.Collections.Generic;
// using System.Threading.Tasks;

// using TurnBasedStrategyFramework.Common.Cells;
// using TurnBasedStrategyFramework.Common.Units;
// using TurnBasedStrategyFramework.Unity.Units;
// using TurnBasedStrategyFramework.Unity.Utilities;

// using UnityEngine;

// namespace XenoSteel.Units
// {
//     public class XenoSteelMoveComponent : UnityMoveComponent
//     {
//         private readonly IUnit _unitReference;

//         public XenoSteelMoveComponent(IUnit unitReference)
//             : base(unitReference)
//         {
//             _unitReference = unitReference;
//         }

//         public override async Task MovementAnimation(
//             IEnumerable<ICell> path,
//             ICell destination)
//         {
//             var currentCell = _unitReference.CurrentCell;

//             var unit =
//                 _unitReference as Component;

//             var facing =
//                 unit?.GetComponent<XenoSteelUnitFacing>();

//             foreach (var cell in path)
//             {
//                 if (facing != null)
//                 {
//                     SetFacing(facing, currentCell, cell);
//                 }

//                 _unitReference.InvokeUnitLeftCell(
//                     new UnitChangedGridPositionEventArgs(
//                         _unitReference,
//                         currentCell,
//                         cell));

//                 while (!_unitReference.WorldPosition.Equals(
//                     cell.WorldPosition))
//                 {
//                     _unitReference.WorldPosition =
//                         Vector3.MoveTowards(
//                             _unitReference.WorldPosition.ToVector3(),
//                             cell.WorldPosition.ToVector3(),
//                             Time.deltaTime *
//                             _unitReference.MovementAnimationSpeed)
//                         .ToIVector3();

//                     await Awaitable.NextFrameAsync();
//                 }

//                 _unitReference.InvokeUnitEnteredCell(
//                     new UnitChangedGridPositionEventArgs(
//                         _unitReference,
//                         currentCell,
//                         cell));

//                 currentCell = cell;
//             }

//             _unitReference.WorldPosition =
//                 destination.WorldPosition;
//         }

//         private void SetFacing(
//             XenoSteelUnitFacing facing,
//             ICell currentCell,
//             ICell nextCell)
//         {
//             Vector3 direction =
//                 nextCell.WorldPosition.ToVector3()
//                 - currentCell.WorldPosition.ToVector3();

//             if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
//             {
//                 facing.SetDirection(
//                     direction.x >= 0f
//                         ? XenoSteelUnitFacing.FacingDirection.Right
//                         : XenoSteelUnitFacing.FacingDirection.Left);
//             }
//             else
//             {
//                 facing.SetDirection(
//                     direction.z >= 0f
//                         ? XenoSteelUnitFacing.FacingDirection.Up
//                         : XenoSteelUnitFacing.FacingDirection.Down);
//             }
//         }
//     }
// }