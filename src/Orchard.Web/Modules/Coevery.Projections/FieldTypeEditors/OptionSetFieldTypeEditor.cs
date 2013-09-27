﻿using System;
using System.Linq;
using Coevery.OptionSet.Models;
using Coevery.Projections.FilterEditors.Forms;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FieldTypeEditors;

namespace Coevery.Projections.FieldTypeEditors {
    public class OptionSetFieldTypeEditor : IConcreteFieldTypeEditor {
        public Localizer T { get; set; }
        public Filter Filter { get; set; }

        public OptionSetFieldTypeEditor() {
            T = NullLocalizer.Instance;
            Filter = ApplyFilter;
        }

        public bool CanHandle(string fieldTypeName, Type storageType) {
            return fieldTypeName == "OptionSetField";
        }

        public void ApplyFilter(FilterContext context, IFieldTypeEditor fieldTypeEditor, string storageName, Type storageType, ContentPartDefinition part, ContentPartFieldDefinition field) {
            var op = (OptionSetOperator) Enum.Parse(typeof (OptionSetOperator), (string) context.State.Operator);
            string value = context.State.Value;
            if (value == null) {
                return;
            }
            var valueArr = value.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            switch (op) {
                case OptionSetOperator.MatchesAny:
                    Action<IAliasFactory> selectorAny = alias => alias.ContentPartRecord<OptionItemContainerPartRecord>().Property("OptionItems", "opits").Property("OptionItemRecord", "opcpr");
                    Action<IHqlExpressionFactory> filterAny = x => x.InG("Id", valueArr);
                    context.Query.Where(selectorAny, filterAny);
                    break;
                case OptionSetOperator.MatchesAll:
                    foreach (var id in valueArr) {
                        var optionId = id;
                        Action<IAliasFactory> selectorAll =
                            alias => alias.ContentPartRecord<OptionItemContainerPartRecord>().Property("OptionItems", "opit" + optionId);
                        Action<IHqlExpressionFactory> filterAll = x => x.Eq("OptionItemRecord.Id", optionId);
                        context.Query.Where(selectorAll, filterAll);
                    }
                    break;
                case OptionSetOperator.NotMatchesAny:
                    Action<IAliasFactory> selectorNotAny = alias => alias.ContentPartRecord<OptionItemContainerPartRecord>().Property("OptionItems", "opits").Property("OptionItemRecord", "opcpr");
                    Action<IHqlExpressionFactory> filterNotAny = x => x.Not(y => y.InG("Id", valueArr));
                    context.Query.Where(selectorNotAny, filterNotAny);
                    break;
            }
        }

        public bool CanHandle(Type storageType) {
            return false;
        }

        public string FormName {
            get { return OptionSetFilterForm.FormName; }
        }

        public Action<IHqlExpressionFactory> GetFilterPredicate(dynamic formState) {
            return null;
        }

        public LocalizedString DisplayFilter(string fieldName, string storageName, dynamic formState) {
            return OptionSetFilterForm.DisplayFilter(fieldName + " " + storageName, formState, T);
        }

        public Action<IAliasFactory> GetFilterRelationship(string aliasName) {
            return null;
        }

    }
}