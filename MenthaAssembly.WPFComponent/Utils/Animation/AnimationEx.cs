using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace MenthaAssembly
{
    public static class AnimationEx
    {
        public static readonly DependencyProperty EnableAnimationInDesignModeProperty =
            DependencyProperty.RegisterAttached("EnableAnimationInDesignMode", typeof(bool), typeof(AnimationEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    int Index = GetAnimationIndex(d);
                    if (e.NewValue is true)
                        OnAnimationIndexChanged(d, new DependencyPropertyChangedEventArgs(AnimationIndexProperty, -1, Index));
                    else
                        OnAnimationIndexChanged(d, new DependencyPropertyChangedEventArgs(AnimationIndexProperty, Index, -1));
                }));
        public static bool GetEnableAnimationInDesignMode(DependencyObject obj)
            => (bool)obj.GetValue(EnableAnimationInDesignModeProperty);
        public static void SetEnableAnimationInDesignMode(DependencyObject obj, bool value)
            => obj.SetValue(EnableAnimationInDesignModeProperty, value);

        public static readonly DependencyProperty AnimationsProperty =
            DependencyProperty.RegisterAttached("Animations", typeof(AnimationCollection), typeof(AnimationEx), new PropertyMetadata());
        public static AnimationCollection GetAnimations(DependencyObject obj)
            => (AnimationCollection)obj.GetValue(AnimationsProperty);
        public static void SetAnimations(DependencyObject obj, AnimationCollection value)
            => obj.SetValue(AnimationsProperty, value);

        public static readonly DependencyProperty AnimationIndexProperty =
            DependencyProperty.RegisterAttached("AnimationIndex", typeof(int), typeof(AnimationEx), new PropertyMetadata(-1, OnAnimationIndexChanged));
        public static int GetAnimationIndex(DependencyObject obj)
            => (int)obj.GetValue(AnimationIndexProperty);
        public static void SetAnimationIndex(DependencyObject obj, int value)
            => obj.SetValue(AnimationIndexProperty, value);

        private static void OnAnimationIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement Element)
            {
                if (!Element.IsLoaded)
                {
                    void OnElementLoaded(object sender, RoutedEventArgs Arg)
                    {
                        Element.Loaded -= OnElementLoaded;
                        OnAnimationIndexChanged(d, e);
                    }
                    Element.Loaded += OnElementLoaded;
                    return;
                }

                if (e.NewValue is int Index &&
                    e.OldValue is int OldIndex &&
                    GetAnimations(d) is AnimationCollection Animations)
                {
                    if (Index == -1)
                    {
                        if (OldIndex != -1 &&
                            Animations.ElementAtOrDefault(OldIndex) is Animation LastAnimation)
                        {
                            foreach (Timeline i in LastAnimation)
                            {
                                DependencyObject Target = Storyboard.GetTarget(i);
                                if (Target is null)
                                {
                                    string TargetName = Storyboard.GetTargetName(i);
                                    if (!string.IsNullOrEmpty(TargetName))
                                        Target = Element.FindName(TargetName) as DependencyObject;
                                }

                                if (Storyboard.GetTargetProperty(i) is PropertyPath Path &&
                                    (Target ?? d).TryGetPropertyByPath(Path, out DependencyProperty Property, out IAnimatable ParentObject))
                                    ParentObject.BeginAnimation(Property, null, HandoffBehavior.SnapshotAndReplace);
                            }
                        }

                        return;
                    }

                    if (!GetEnableAnimationInDesignMode(d) &&
                        DesignerProperties.GetIsInDesignMode(d))
                        return;

                    Animation Animation = Animations[Index];
                    FreezableCollection<Timeline> CloneAnimation = Animation.CloneCurrentValue();
                    CloneAnimation.Freeze();

                    Storyboard Story = new Storyboard { Children = new TimelineCollection(CloneAnimation) };
                    Story.Completed += (d, e) => Animation.OnCompleted();
                    Story.Freeze();

                    Element.BeginStoryboard(Story, HandoffBehavior.SnapshotAndReplace);
                }
            }
        }

        public static void Restart(DependencyObject d)
        {
            int Index = GetAnimationIndex(d);
            if (Index != -1)
                OnAnimationIndexChanged(d, new DependencyPropertyChangedEventArgs(AnimationIndexProperty, Index, Index));
        }

    }
}
