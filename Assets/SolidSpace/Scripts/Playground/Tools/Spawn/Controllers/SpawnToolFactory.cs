using SolidSpace.Playground.Core;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class SpawnToolFactory : ISpawnToolFactory
    {
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IUIFactory _uiFactory;
        private readonly IPlaygroundUIManager _playgroundUI;

        public SpawnToolFactory(IUIManager uiManager,
                                IPointerTracker pointer,
                                IUIFactory uiFactory,
                                IPlaygroundUIManager playgroundUI)
        {
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
            _playgroundUI = playgroundUI;
        }
        
        public ISpawnTool Create(ISpawnToolHandler handler)
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Spawn");

            var radiusField = _uiFactory.CreateStringField();
            radiusField.SetLabel("Radius");
            radiusField.SetValue("0");
            radiusField.SetValueCorrectionBehaviour(new IntMaxBehaviour(0));
            window.AttachChild(radiusField);

            var amountField = _uiFactory.CreateStringField();
            amountField.SetLabel("Amount");
            amountField.SetValue("1");
            amountField.SetValueCorrectionBehaviour(new IntMaxBehaviour(1));
            window.AttachChild(amountField);

            var randomRotationTag = _uiFactory.CreateTagLabel();
            randomRotationTag.SetLabel("Random rotation");
            randomRotationTag.SetState(ETagLabelState.Negative);
            window.AttachChild(randomRotationTag);

            var tool = new SpawnTool
            {
                UIManager = _uiManager,
                Pointer = _pointer,
                Window = window,
                SpawnRadiusField = radiusField,
                SpawnAmountField = amountField,
                SpawnAmount = 1,
                SpawnRadius = 0,
                PositionGenerator = new PositionGenerator(),
                RotationGenerator = new RotationGenerator(),
                Handler = handler,
                PlaygroundUI = _playgroundUI,
                RotationIsRandom = false,
                RotationLabel = randomRotationTag,
                PointerClickPosition = float2.zero,
                IsPlacingMode = false,
                PreviousPointerAngle = 0
            };

            radiusField.ValueChanged += () => tool.SpawnRadius = int.Parse(tool.SpawnRadiusField.Value);
            amountField.ValueChanged += () => tool.SpawnAmount = int.Parse(tool.SpawnAmountField.Value);
            randomRotationTag.Clicked += () =>
            {
                tool.RotationIsRandom = !tool.RotationIsRandom;
                randomRotationTag.SetState(tool.RotationIsRandom ? ETagLabelState.Positive : ETagLabelState.Negative);
            };

            return tool;
        }
    }
}