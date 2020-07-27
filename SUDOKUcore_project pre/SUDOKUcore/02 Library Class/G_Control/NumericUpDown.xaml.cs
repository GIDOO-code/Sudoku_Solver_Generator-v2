using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using static System.Diagnostics.Debug;
using System.Windows.Controls;

namespace GIDOO_space{
    public delegate void GIDOOEventHandler( object sender, GIDOOEventArgs args );
 
    public class GIDOOEventArgs: EventArgs{
	    public string eName;
	    public int    eValue;

	    public GIDOOEventArgs( string eName=null, int eValue=-1 ){
            try{
		        this.eName = eName;
		        this.eValue = eValue;
            }
            catch(Exception e ){ WriteLine(e.Message); WriteLine(e.StackTrace); }
	    }
    }
}

namespace GIDOO_space{
    public partial class NumericUpDown: UserControl{
        public event   GIDOOEventHandler NumUDValueChanged; 

        public static readonly DependencyProperty ValueProperty=DependencyProperty.Register(
            "Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));
        public static readonly DependencyProperty MinValueProperty=DependencyProperty.Register(
            "MinValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));
        public static readonly DependencyProperty MaxValueProperty=DependencyProperty.Register(
            "MaxValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));
        public static readonly DependencyProperty IncrementProperty=DependencyProperty.Register(
            "Increment", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));

        public int Value{
            get{ return (int)GetValue(ValueProperty); }
            set{ SetValue(ValueProperty, value); }
        }
        public int MinValue{
            get{ return (int)GetValue(MinValueProperty); }
            set{ SetValue(MinValueProperty, MinValue); }
        }
        public int MaxValue{
            get{ return (int)GetValue(MaxValueProperty); }
            set{ SetValue(MaxValueProperty, MaxValue); }
        }
        public int Increment{
            get{ return (int)GetValue(IncrementProperty); }
            set{ SetValue(IncrementProperty, Increment); }
        }

        public NumericUpDown(){
            InitializeComponent();
        }

        private void UpButton_Click(object sender, RoutedEventArgs e){
            int inc=(Increment>1)? Increment: 1;
            int k = Value+inc;
            Value = Math.Min(k,MaxValue);
        }
        private void DownButton_Click(object sender, RoutedEventArgs e){
            int inc=(Increment>1)? Increment: 1;
            int k = Value-inc;
            Value = Math.Max(k,MinValue);
        }

        private void textBoxValue_TextChanged(Object sender,TextChangedEventArgs e){        
            int k=textBoxValue.Text.ToInt();
            k = Math.Min(k,MaxValue);
            k = Math.Max(k,MinValue);
            Value=k;
            textBoxValue.Text=k.ToString();

            if(NumUDValueChanged!=null){
                NumUDValueChanged( this, new GIDOOEventArgs( "TextChanged", Value ));
            }
        }
    }
}
