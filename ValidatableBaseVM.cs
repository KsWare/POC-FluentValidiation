using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace FluentValidiationPoc {

	public abstract class ValidatableBaseVM : BaseVM, INotifyDataErrorInfo {

		protected override bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
			if (!base.SetValue(ref field, value, propertyName)) return false;
			Validate();
			return true;
		}

		protected virtual bool SetValue<T>(ref T field, T value, ValidationMode validationMode, [CallerMemberName] string propertyName = null) {
			if (!base.SetValue(ref field, value, propertyName)) return false;
			switch (validationMode) {
				case ValidationMode.Single: Validate(propertyName); break;
				case ValidationMode.All   : Validate(); break;
				default: throw new InvalidOperationException();
			}
			return true;
		}

		protected virtual bool SetValue<T>(ref T field,T value,ValidationMode validationMode, params string[] propertyNames) {
			if (!base.SetValue(ref field, value, propertyNames[0])) return false;
			switch (validationMode) {
				case ValidationMode.Single: Validate(propertyNames[0]); break;
				case ValidationMode.All: Validate(); break;
				case ValidationMode.Multi: Validate(propertyNames); break;
				default: throw new InvalidOperationException();
			}
			return true;
		}

		#region Validation

		public IValidator Validator { get; set; }

		/// <summary>
		/// Validates the specified properties.
		/// </summary>
		/// <param name="propertyNames">The property names.</param>
		public void Validate(params string[] propertyNames) {
			var a = ValidationResults.Select(e => e.PropertyName).Distinct();
			if (Validator == null) {
				ValidationResults = ValidationResults.Except(propertyNames).ToList();
			}
			else {
				var validationSelector = propertyNames.Any()
					? ValidatorOptions.ValidatorSelectors.MemberNameValidatorSelectorFactory(propertyNames)
					: ValidatorOptions.ValidatorSelectors.DefaultValidatorSelectorFactory();
				var validationContext = new ValidationContext(this, new PropertyChain(), validationSelector);
				var results = Validator.Validate(validationContext).Errors;
				if (propertyNames.Length == 0) ValidationResults = results;
				else ValidationResults = ValidationResults.Except(propertyNames).Concat(results).ToList();
			}
			OnPropertyChanged(nameof(ValidationResults));				
			var b = ValidationResults.Select(e => e.PropertyName).Distinct();
			foreach (var propertyName in a.Concat(b).Distinct()) OnErrorsChanged(propertyName);
		}

		public IList<ValidationFailure> ValidationResults { get; private set; } = new List<ValidationFailure>();

		#endregion

		#region INotifyDataErrorInfo

		public IEnumerable GetErrors(string propertyName) => ValidationResults.Where(e=>e.PropertyName ==propertyName).OrderBy(e=>e.Severity).FirstOrDefault()?.ErrorMessage;

		public bool HasErrors => ValidationResults.Count > 0;

		protected void OnErrorsChanged([CallerMemberName] string propertyName = null) =>
			ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		#endregion
	}

	internal static class ValidationFailureListExtension {

		public static IEnumerable<ValidationFailure> Except(this IEnumerable<ValidationFailure> list, string[] propertyNames) {
			if (propertyNames == null || propertyNames.Length == 0) return new ValidationFailure[0];
			return list.Where(item => !propertyNames.Contains(item.PropertyName));
		}
	}
}