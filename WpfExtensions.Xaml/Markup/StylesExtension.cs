using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using WpfExtensions.Xaml.ExtensionMethods;

namespace WpfExtensions.Xaml.Markup
{
    [MarkupExtensionReturnType(typeof(Style))]
    public class StylesExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private static Style MergeStyles(Style left, Style right)
        {
            if (TryGetNonNull(left, right, out var value)) return value;

            var result = new Style(
                GetMergedStyleType(left.TargetType, right.TargetType),
                MergeStyles(left.BasedOn, right.BasedOn))
            {
                // Merge Resources
                Resources = MergeResources(left.Resources, right.Resources)
            };

            // Merge Setters
            result.Setters.AddRange(MergeSetters(left.Setters, right.Setters));

            // Merge Triggers
            result.Triggers.AddRange(MergeTriggers(left.Triggers, result.Triggers));

            result.Seal();
            return result;
        }

        private static Type GetMergedStyleType(Type left, Type right)
        {
            if (left == right) return left;
            if (left.IsAssignableFrom(right)) return right;
            if (right.IsAssignableFrom(left)) return left;

            throw new ArgumentException();
        }

        private static ResourceDictionary MergeResources(ResourceDictionary left, ResourceDictionary right)
        {
            if (TryGetNonNull(left, right, out var value)) return value;

            var result = new ResourceDictionary();
            result.MergedDictionaries.Add(left);
            result.MergedDictionaries.Add(right);

            return result;
        }

        private static IEnumerable<Setter> MergeSetters(SetterBaseCollection left, SetterBaseCollection right)
        {
            return TryGetNonNull(left, right, out var value)
                ? value?.Cast<Setter>()
                : right.Cast<Setter>().Union(left.Cast<Setter>(), (x, y) =>
                    string.IsNullOrEmpty(y.TargetName)
                        ? Equals(x.Property, y.Property)
                        : Equals(x.TargetName, y.TargetName) && Equals(x.Property, y.Property));
        }

        private static IEnumerable<TriggerBase> MergeTriggers(TriggerCollection left, TriggerCollection right)
        {
            if (TryGetNonNull(left, right, out var value)) return value;

            return right.Union(left, (x, y) =>
            {
                switch (y)
                {
                    case Trigger trigger:
                        //trigger.Setters
                        break;
                    case DataTrigger dataTrigger:
                        //dataTrigger.Setters
                        break;
                    case MultiTrigger multiTrigger:
                        //multiTrigger.Setters
                        break;
                    case MultiDataTrigger multiDataTrigger:
                        //multiDataTrigger.Setters
                        break;
                    case EventTrigger eventTrigger:
                        break;
                }

                return false;
            });
        }

        private static IEnumerable<TriggerAction> MergeActions(TriggerActionCollection left, TriggerActionCollection right)
        {
            if (TryGetNonNull(left, right, out var value)) return value;

            var result = new List<TriggerAction>(right);

            // TODO	
            foreach (var triggerAction in left)
            {
            }

            return result;
        }

        private static bool TryGetNonNull<T>(T left, T right, out T result) where T : class
        {
            if (Equals(left, right))
            {
                result = left;
                return true;
            }

            if (left == null)
            {
                result = right;
                return true;
            }

            if (right == null)
            {
                result = left;
                return true;
            }

            result = null;
            return false;
        }

        private static IEnumerable<T> Union<T>(IEnumerable<T> left, IEnumerable<T> right, Func<T, T, bool> equals, Func<T, T, T> merge)
        {
            var rightArray = right.ToArray();
            var result = new List<T>(rightArray);

            foreach (var leftItem in left)
            {
                var rightItem = rightArray.FirstOrDefault(item => equals(item, leftItem));

                if (rightItem == null)
                {
                    result.Add(leftItem);
                }
                else
                {
                    var mergedItem = merge(leftItem, rightItem);
                    if (mergedItem != null && result.Remove(rightItem))
                    {
                        result.Add(mergedItem);
                    }
                }
            }

            return result;
        }

        private static bool Equals<T>(T left, T right) => EqualityComparer<T>.Default.Equals(left, right);
    }
}