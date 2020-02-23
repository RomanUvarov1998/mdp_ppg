using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MDP_PPG.EntitiesEditing
{
	/// <summary>
	/// Логика взаимодействия для UC_ValidativeTextBox.xaml
	/// </summary>
	public partial class UC_ValidativeTextBox : UserControl, INotifyPropertyChanged
	{
		public UC_ValidativeTextBox()
		{
			InitializeComponent();

			DataContext = this;
		}

		public void SetValues(
			string initialValue,
			Func<string, bool> validationFunction,
			Func<string, string> coerseFunction,
			Action setBtnOkEnabled)
		{
			MyNotifyPropertyChanged(nameof(FieldName));
			MyNotifyPropertyChanged(nameof(ErrMsgText));
			//MyNotifyPropertyChanged(nameof(NonUniqueErrMsg));

			ValidationFunction = validationFunction;
			CoerseFunction = coerseFunction;
			//CheckIfUnique = checkIfUnique;
			SetBtnOkEnabled = setBtnOkEnabled;

			MyText = initialValue;
			//ValueBeforeEditing = initialValue;
			IsValid = ValidationFunction.Invoke(_myText);
			State = IsValid ? States.Valid : States.BeforeFirstUserInput;
		}

		#region uniqueness
		//----------------------------------------- Uniqueness --------------------------------------------
		//public async Task<bool> ValidateAsUniqueOnSavingData()
		//{
		//	if (string.Equals(ValueBeforeEditing, MyText))
		//		return true;

		//	IsCheckingAsUnique = true;
		//	IsNotUniqueButMustBe = await Task.Run(() => !CheckIfUnique(MyText));
		//	IsCheckingAsUnique = false;

		//	return !IsNotUniqueButMustBe;
		//}
		//private bool IsCheckingAsUnique
		//{
		//	get => _isCheckingAsUnique;
		//	set
		//	{
		//		_isCheckingAsUnique = value;
		//		MyNotifyPropertyChanged(nameof(UniquenessMsgVisibility));
		//		MyNotifyPropertyChanged(nameof(IsTextBoxEnabled));
		//		MyNotifyPropertyChanged(nameof(UniquenessMsg));
		//	}
		//}
		//private bool IsNotUniqueButMustBe = false;
		//private string ValueBeforeEditing;
		//public Visibility UniquenessMsgVisibility => IsNotUniqueButMustBe || IsCheckingAsUnique ? Visibility.Visible : Visibility.Collapsed;
		//public string UniquenessMsg => IsCheckingAsUnique ? "Проверка уникальности..." : NonUniqueErrMsg;
		//public bool IsTextBoxEnabled => !IsCheckingAsUnique;
		//public string NonUniqueErrMsg { get; set; }
		#endregion

		//----------------------------------------- Immediately validation --------------------------------------------

		public string MyText
		{
			get => _myText;
			set
			{
				_myText = value;
				MyNotifyPropertyChanged(nameof(MyText));
				SetStateOnInput();
			}
		}
		public string ErrMsgText { get; set; }
		public Visibility ErrMsgVisibility => State == States.NotValid ? Visibility.Visible : Visibility.Collapsed;
		public Brush MyBorderBrush => State == States.NotValid ? MySettings.RedMark : Brushes.Transparent;
		enum States
		{
			BeforeFirstUserInput,
			NotValid,
			Valid
		}
		private States State
		{
			get => _state;
			set
			{
				_state = value;

				MyNotifyPropertyChanged(nameof(ErrMsgVisibility));
				MyNotifyPropertyChanged(nameof(MyBorderBrush));

				SetBtnOkEnabled?.Invoke();
			}
		}
		private void SetStateOnInput()
		{
			switch (State)
			{
				case States.BeforeFirstUserInput:
					Validate();
					break;

				case States.NotValid:
					Validate();
					break;

				case States.Valid:
					Validate();
					break;
			}
		}
		private void Validate()
		{
			IsValid = ValidationFunction.Invoke(MyText);
			State = IsValid ? States.Valid : States.NotValid;
		}
		private bool IsValid;
		public bool AllowsSavingInput => State == States.Valid;


		//----------------------------------------- UI info --------------------------------------------
		public string FieldName { get; set; }


		//----------------------------------------- Coersing --------------------------------------------
		private void Tb_LostFocus(object sender, RoutedEventArgs e)
		{
			if (CoerseFunction != null)
				MyText = CoerseFunction.Invoke(MyText);
		}


		//----------------------------------------- Other members --------------------------------------------
		private Func<string, bool> ValidationFunction;
		private Func<string, string> CoerseFunction;
		private Action SetBtnOkEnabled;


		public event PropertyChangedEventHandler PropertyChanged;
		private void MyNotifyPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}


		private string _myText;
		private States _state;
	}
}
