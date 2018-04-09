using System.Windows.Controls;
using FluentValidation;

namespace FluentValidiationPoc {
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class UserControl1 : UserControl {
		public UserControl1() {
			InitializeComponent();
			DataContext = new UserControl1VM();
		}
	}

	public class UserControl1VM : ValidatableBaseVM {

		private string _a, _b, _c, _d;

		public UserControl1VM() {
			Validator = new MyValidator();
		}

		public string A { get => _a; set => SetValue(ref _a, value, ValidationMode.Multi, nameof(A),nameof(B)); }

		public string B { get => _b; set => SetValue(ref _b, value, ValidationMode.Multi, nameof(B), nameof(A)); }

		public string C { get => _c; set => SetValue(ref _c, value, ValidationMode.Single); }

		public string D { get => _d; set => SetValue(ref _d, value); }

		public class MyValidator : AbstractValidator<UserControl1VM> {
			public MyValidator() {
//				CascadeMode = CascadeMode.StopOnFirstFailure;

				RuleFor(vm => vm.A).NotEmpty().Equal(vm => vm.B);
				RuleFor(vm => vm.B).NotEmpty().Equal(vm => vm.A);
				RuleFor(vm => vm.C).NotEmpty();
				RuleFor(vm => vm.D).NotEmpty();
			}
		}
		
	}

	public enum ValidationMode {
		Single,
		Multi,
		All,
	}

}
