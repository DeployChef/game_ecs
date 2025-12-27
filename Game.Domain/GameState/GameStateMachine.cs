using Game.Domain.ECS;
using Game.Domain.Exceptions;

namespace Game.Domain.GameState;

/// <summary>
/// Машина состояний игры - управляет переходами между состояниями.
/// 
/// Состояния:
/// - Initialized → DrawingHand (начало хода)
/// - DrawingHand → SelectingCards (карты взяты)
/// - SelectingCards → PlayingHand (карты выбраны)
/// - PlayingHand → HandComplete (подсчет завершен)
/// - HandComplete → DrawingHand (новый ход) или EndTurn (конец раунда)
/// </summary>
public class GameStateMachine
{
    public GameStateType CurrentState { get; private set; }
    public World World { get; }
    public Entity HandEntity { get; }

    public GameStateMachine(World world, Entity handEntity)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        HandEntity = handEntity;
        CurrentState = GameStateType.Initialized;
    }

    /// <summary>
    /// Переход в состояние DrawingHand (начало хода).
    /// </summary>
    public void StartTurn()
    {
        if (CurrentState != GameStateType.Initialized && 
            CurrentState != GameStateType.HandComplete && 
            CurrentState != GameStateType.EndTurn)
        {
            throw new InvalidStateTransitionException(
                $"Cannot start turn from state {CurrentState}. Expected Initialized, HandComplete, or EndTurn.");
        }

        CurrentState = GameStateType.DrawingHand;
    }

    /// <summary>
    /// Переход в состояние SelectingCards (карты взяты в руку).
    /// </summary>
    public void CardsDrawn()
    {
        if (CurrentState != GameStateType.DrawingHand)
        {
            throw new InvalidStateTransitionException(
                $"Cannot transition to SelectingCards from state {CurrentState}. Expected DrawingHand.");
        }

        CurrentState = GameStateType.SelectingCards;
    }

    /// <summary>
    /// Переход в состояние PlayingHand (карты выбраны для игры).
    /// </summary>
    public void CardsSelected()
    {
        if (CurrentState != GameStateType.SelectingCards)
        {
            throw new InvalidStateTransitionException(
                $"Cannot transition to PlayingHand from state {CurrentState}. Expected SelectingCards.");
        }

        CurrentState = GameStateType.PlayingHand;
    }

    /// <summary>
    /// Переход в состояние HandComplete (рука завершена).
    /// </summary>
    public void HandCompleted()
    {
        if (CurrentState != GameStateType.PlayingHand)
        {
            throw new InvalidStateTransitionException(
                $"Cannot transition to HandComplete from state {CurrentState}. Expected PlayingHand.");
        }

        CurrentState = GameStateType.HandComplete;
    }

    /// <summary>
    /// Переход в состояние EndTurn (конец хода).
    /// </summary>
    public void EndTurn()
    {
        if (CurrentState != GameStateType.HandComplete)
        {
            throw new InvalidStateTransitionException(
                $"Cannot end turn from state {CurrentState}. Expected HandComplete.");
        }

        CurrentState = GameStateType.EndTurn;
    }
}

