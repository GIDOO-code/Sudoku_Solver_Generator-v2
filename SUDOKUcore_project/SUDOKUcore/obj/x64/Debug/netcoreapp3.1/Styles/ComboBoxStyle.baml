   M S B A M L   `   `   `  ����W  SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� 
TargetType �� Property �� Value �� 
TargetType'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard �� 
TargetName �� TargetProperty8   2System.Windows.Media.Animation.EasingColorKeyFrame	 �� KeyTime   DisabledForegroundColor
 �� Value R� KeyTime   System.Windows.Visibility   Visible R� Value   Hidden d�Name  ComboBoxToggleButton �� 	ClickMode d� 	IsChecked �� Mode �� RelativeSource ��Name J� IsHitTestVisible �� SelectionBoxItem �� SelectionBoxItemTemplate ��Name  ComboBoxTextBox �� 
IsReadOnly �� 
IsReadOnly �Name �� IsDropDownOpen � AllowsTransparency �Name J� SnapsToDevicePixels  � ActualWidth! �� MaxDropDownHeight" ��Name  BorderMediumColor# �� Color  ControlLightColor$ K� IsItemsHost-  'System.Windows.Input.KeyboardNavigation%   DirectionalNavigation& �� Triggers' P� Property( P� Value) �� 
TargetName* P� 
SourceName+ �� Padding  SelectedBackgroundColor  SelectedUnfocusedColor  WindowColor  ContentAreaColorLight 	 ContentAreaColorDark 
 DisabledControlLightColor  DisabledControlDarkColor  ControlMediumColor  ControlDarkColor  ControlMouseOverColor  ControlPressedColor  
GlyphColor  GlyphMouseOver  BorderLightColor  BorderDarkColor  PressedBorderLightColor  PressedBorderDarkColor  DisabledBorderLightColor  DisabledBorderDarkColor  DefaultBorderBrushDarkColor  HeaderTopColor " DatagridCurrentCellBorderColor  SliderTrackDarkColor  NavButtonFrameColor  MenuPopupBrush, �� EndPoint- �� 
StartPoint. � Color/ � Offset $  ProgressBarIndicatorAnimatedFill�� 5      F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      :x,http://schemas.microsoft.com/winfx/2006/xaml      5      %�  '��       7   7  7  '�� �
    7  7  &	 �    &	 	    &		 *    &	
 K    &	 l    &	  �    &	 �    &	 �    &	 �    &	     &	 .    &	 O    &	 p    &	 �    &	 �    &	 �    &	     &	 7    &	 X    &	 y    &	 �    &	 �    &	 �    &	 �    &	     &	 7    &	 X    &	 y    &	 ,    �� 5      "  ��6   .�5   
   ��  �@J�SnapsToDevicePixels$
 true��63   �� 5   
    �@�OverridesDefaultStyle$
 true��63   �� 5   
    � ��$
 Auto=�6G   �� 5   
    � ��$
 Auto=�6G   �� 5	   
    � ��$
 true��6C   �� 5
   
    � ��$	 120��6.   �� 5   
    � ��$ 20��6/   �� 5   
    � �� 5      �� 5      " ��6"   .R�5      �  5        5      CommonStates��. 5   "     
Normal��6.     5   "   	MouseOver��6.     5   "   Disabled��. 5   &   �� .��5   *   �� $ PART_EditableTextBox��6G   $6 0(TextElement.Foreground).(SolidColorBrush.Color)�5   1   .g�5   .     $	 0��6B   8
   6N   5   +   5   '   5   #   5        5      
EditStates��. 5   "     Editable��. 5   &   �� .��5    *   T� $ (UIElement.Visibility)�6H   $ PART_EditableTextBox��5!   2   ..�5"   .   i� $ 0��6E   # Z 5#   -   5$   +   T� 5%   *   $ (UIElement.Visibility)�6K   $ ContentSite��5&   2   ..�5'   .   i� $ 0��6E   # Z 6R   5(   +   5)   '   5*   #     5+   "   
Uneditable��6.   5,      .+�5.      d� $ ToggleButton��6'   8�� $��2��5/   J   $|�false��6Z   $ Press=�6l    50   "   �� *IsDropDownOpen+$ TwoWay=� �� *TemplatedParent+�� 51      $ ContentSite��6+   $ False��52      #��z 53      #��z 54      #��z��55      $��3,3,23,3q�56      $��Stretch=�57      $
��Left=�58      59      �� 5:      $ PART_EditableTextBox��6"   8�� $
��Left=�5<       $��Bottom=�5=       $��3,3,23,3q�5>       $
|�True��5?       
������ $y�Hidden=�5A       # z 5B       ��5:   @   Z� 5B       � 5C      $ Popup��6    $��Bottom=�5D   "   #��z 5E   "   $
 True��5F   "   $|�False��5G   "   $��Slide=�5H   "   .��5I      � $ DropDown��6#   $
 True��5J   #   #��z  5K   #   #��z! 5L   #   .+�5M   "   �� $" DropDownBorder��6)   $��1q�6A   ��5N   &   �� 5O   *   ## �  6:   ��5Q   &   �� 5R   *   ## �  6:   5T   #   �� 5U   "   $��4,6,4,6q�6/   $
 True��6@   .��5V   &   �� $
$ True��61   $% 	Contained=�6D   5W   #   5X      5Y      5Z      & 5[      P� 5\      ' �@��HasItems$( false��66   . �5]      �� $) DropDownBorder��6%    � ��$ 95��6V   5^      P� 5_      ' �@��
IsGrouping$
( true��68   . �5`      ��  � ��$ false��6N   5a      P� 5b      $* Popup��6"   ' �@�AllowsTransparency$
( true��6S   . �5c      �� $) DropDownBorder��6%    �@��CornerRadius$ 4��6Y   �� 5d      $) DropDownBorder��6%    � ��$ 0,2,0,0q�6S   5e      5g      5i      5j      �� 5m      "  ��6   .�5n   
   ��  �@J�SnapsToDevicePixels$
 true��60   �� 5o   
    �@�OverridesDefaultStyle$
 true��62   �� 5p   
    � �� 5q      �� 5r      " ��6"   .R�5s      �� $" Border��6   $+ 2q�6-   $
 true��69   
������  5t        5u      SelectionStates��. 5v   "     
Unselected��6.     5w   "   Selected��. 5x   &   �� .��5y   *   �� $ Border��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�5z   1   .g�5{   .     $	 0��6B   8
   6N   5|   +   5}   '   5~   #     5   "   SelectedUnfocused��. 5�   &   �� .��5�   *   �� $ Border��6G   $0 *(Panel.Background).(SolidColorBrush.Color)�5�   1   .g�5�   .     $	 0��6B   8
  6N   5�   +   5�   '   5�   #   5�      ._�5�      �� 5�      5�      5�      5�      ��5�      	#FFE8EDF9��6+   ��5�      	#FFC5CBF9��65   ��5�      	#FF7381F9��64   ��5�      	#FFE8EDF9��69   ��5�      	#FFC5CBF9��68   ��5�      	#FF888888��67   ��5�      	#FFC5CBF9��67   ��5�      	#FFDDDDDD��66   ��5�      	White��6-   ��5�      	#FF7381F9��62   ��5�      	#FF211AA9��60   ��5�      	#FF3843C4��65   ��5�      	#FF211AA9��63   ��5�      	#FF999999��6*   ��5�      /+sc#1, 0.004391443, 0.002428215, 0.242281124��6P   ��5�      	#FFCCCCCC��60   ��5�      	#FF888888��61   ��5�      	#FF999999��6/   ��5�      	#FF888888��67   ��5�      	#FF999999��66   ��5�      	#FFAAAAAA��68   ��5�      	#FF888888��67   ��5�      	Black��67   ��5�      	#FFC5CBF9��6.   ��5�      	Black��6:   ��5�      	#FFC5CBF9��64   ��5�      	#FF3843C4��63   �� 5�      $, 0.5,1&�61   $- 0.5,0&�6B   .��5�   
   � #. �  6   $/ 0Q�6C   � 5�   
   #. �  6   $	/ 0.5Q�6D   � 5�   
   #. �  6   $/ 1Q�6D   5�      �� 5�      $	- 0,0&�6C   $	, 1,0&�6T   ��5�      � � 5�      $. 	#000000FF��6   $/ 0Q�61   � 5�      $. 	#600000FF��6   $	/ 0.4Q�61   � 5�      $. 	#600000FF��6   $	/ 0.6Q�61   � 5�      $. 	#000000FF��6   $/ 1Q�61   5�      5�      5"     