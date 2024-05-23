// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsCustomIntegerNumberBox(int defaultValue, int minValue, int maxValue) : SettingsItem<int>
    {
        protected override Drawable CreateControl() => new NumberControl(defaultValue, minValue, maxValue)
        {
            RelativeSizeAxes = Axes.X,
        };

        private sealed partial class NumberControl : CompositeDrawable, IHasCurrentValue<int>
        {
            private readonly BindableWithCurrent<int> current = new BindableWithCurrent<int>();

            public Bindable<int> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            public NumberControl(int defaultValue, int minValue, int maxValue)
            {
                AutoSizeAxes = Axes.Y;

                OutlinedNumberBox numberBox;

                InternalChildren = new[]
                {
                    numberBox = new OutlinedNumberBox(Current, defaultValue)
                    {
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };

                numberBox.Current.BindValueChanged(e =>
                {
                    if (string.IsNullOrEmpty(e.NewValue))
                    {
                        Current.Value = defaultValue;
                        return;
                    }

                    if (int.TryParse(e.NewValue, out int intVal) && intVal >= minValue && intVal <= maxValue)
                        Current.Value = intVal;
                    else
                        numberBox.NotifyInputError();

                    // trigger Current again to either restore the previous text box value, or to reformat the new value via .ToString().
                    Current.TriggerChange();
                });

                Current.BindValueChanged(e =>
                {
                    if (e.NewValue != defaultValue)
                        numberBox.Current.Value = e.NewValue.ToString();
                    else
                        numberBox.Current.Value = null;
                });
            }
        }

        private partial class OutlinedNumberBox : OutlinedTextBox
        {
            private Bindable<int> bindable;
            private int defaultValueBox;

            public OutlinedNumberBox(Bindable<int> bindableNumber, int defaultValue)
            {
                bindable = bindableNumber;
                defaultValueBox = defaultValue;
            }

            protected override bool AllowIme => false;

            protected override bool CanAddCharacter(char character) => char.IsAsciiDigit(character);

            protected override void OnFocusLost(FocusLostEvent e)
            {
                base.OnFocusLost(e);
                if (string.IsNullOrEmpty(Current.Value) && bindable.Value != defaultValueBox)
                {
                    bindable.Value = defaultValueBox;
                    bindable.TriggerChange();
                }
            }

            public new void NotifyInputError() => base.NotifyInputError();
        }
    }
}
