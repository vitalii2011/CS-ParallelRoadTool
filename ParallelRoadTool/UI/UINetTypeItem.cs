using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetTypeItem : UIPanel
    {
        private const int TextFieldWidth = 65;
        private const int LabelWidth = 250;
        private const float ColumnPadding = 8f;
        private const int ReverseButtonWidth = 36;

        public NetInfo NetInfo;        
        public int Index;
        public string FilterText = string.Empty;
        public bool IsReversed;
        public float HorizontalOffset;
        public float VerticalOffset;

        private UILabel Label { get; set; }
        private UITextField HorizontalOffsetField { get; set; }
        private UITextField VerticalOffsetField { get; set; }
        private UITextField DropdownFilterField { get; set; }
        private UIButton DeleteButton { get; set; }
        private UIButton AddButton { get; set; }
        public UICheckBox ReverseCheckbox { get; set; }
        public UIDropDown DropDown { get; private set; }
        public bool IsCurrentItem { get; set; }
        public bool IsFiltered { get; set; }

        private bool Populated { get; set; }        

        public Action OnChangedCallback { get; set; }
        public Action OnDeleteCallback { private get; set; }
        public Action OnAddCallback { private get; set; }

        private MethodInfo _openPopup = typeof(UIDropDown).GetMethod("OpenPopup", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Start()
        {
            name = "PRT_NetTypeItem";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(255, 255, 255, 255);
            size = new Vector2(500 - 8 * 2 - 4 * 2, 40);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = new RectOffset(4,4,0,0);
            autoLayoutStart = LayoutStart.BottomLeft;

            var panel = AddUIComponent<UIPanel>();
            panel.size = new Vector2(size.x, 40);
            panel.relativePosition = Vector2.zero;

            DropDown = UIUtil.CreateDropDown(panel);
            DropDown.width = LabelWidth;
            DropDown.relativePosition = Vector2.zero;
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;            

            ReverseCheckbox = UIUtil.CreateCheckBox(panel, "Reverse", Locale.Get("PRT_TOOLTIPS", "ReverseToggleButton"), false);
            ReverseCheckbox.relativePosition = new Vector3(LabelWidth + ColumnPadding, 2);
            ReverseCheckbox.eventCheckChanged += ReverseCheckboxOnEventCheckChanged;

            HorizontalOffsetField = UIUtil.CreateTextField(panel);
            HorizontalOffsetField.relativePosition = new Vector3(LabelWidth + 2 * ColumnPadding + ReverseButtonWidth, 10);
            HorizontalOffsetField.width = TextFieldWidth;
            HorizontalOffsetField.eventTextSubmitted += HorizontalOffsetField_eventTextSubmitted;

            VerticalOffsetField = UIUtil.CreateTextField(panel);
            VerticalOffsetField.relativePosition = new Vector3(LabelWidth + 3 * ColumnPadding + ReverseButtonWidth + TextFieldWidth, 10);
            VerticalOffsetField.width = TextFieldWidth;
            VerticalOffsetField.eventTextSubmitted += VerticalOffsetField_eventTextSubmitted;

            DeleteButton = UIUtil.CreateUiButton(panel, string.Empty, Locale.Get("PRT_TOOLTIPS", "RemoveNetworkButton"), new Vector2(36, 36), "Remove");
            DeleteButton.zOrder = 0;
            DeleteButton.textScale = 0.8f;
            DeleteButton.relativePosition = new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            AddButton = UIUtil.CreateUiButton(panel, string.Empty, Locale.Get("PRT_TOOLTIPS", "AddNetworkButton"), new Vector2(36, 36), "Add");
            AddButton.zOrder = 1;
            AddButton.isVisible = false;
            AddButton.textScale = 0.8f;
            AddButton.relativePosition = new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);            

            DropdownFilterField = UIUtil.CreateTextField(this);
            DropdownFilterField.size = new Vector2(panel.size.x - 8, panel.size.y);
            DropdownFilterField.relativePosition = Vector2.zero;
            DropdownFilterField.zOrder = DropDown.zOrder + 1;
            DropdownFilterField.eventTextChanged += DropdownFilterField_eventTextChanged;            
            //DropdownFilterField.isVisible = false;

            Label = panel.AddUIComponent<UILabel>();
            Label.textScale = .8f;
            Label.text = "Select a network";
            Label.autoSize = false;
            Label.width = LabelWidth;
            Label.relativePosition = new Vector3(10, 12);
            Label.isVisible = false;            

            DeleteButton.eventClicked += DeleteButton_eventClicked;
            AddButton.eventClicked += AddButton_eventClicked;

            RenderItem();
        }        

        private void PopulateDropDown()
        {
            var items = ParallelRoadTool.AvailableRoadTypes
                .Select(ni => ni.GenerateBeautifiedNetName());
            if (!string.IsNullOrEmpty(FilterText))
            {
                items = items.Where(i => i.ToLowerInvariant().Contains(FilterText.ToLowerInvariant()));
                IsFiltered = true;
            }
            DropDown.items = items.ToArray();
            DropDown.selectedIndex = 0;
            Populated = true;

            DebugUtils.Log($"UINetTypeItem.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");
        }

        public void RenderItem()
        {
            DebugUtils.Log($"RenderItem {NetInfo} at {HorizontalOffset}/{VerticalOffset}");
            if (NetInfo != null)
                Label.text = NetInfo.GenerateBeautifiedNetName();

            if (!Populated) PopulateDropDown();

            DropdownFilterField.text = FilterText;
            HorizontalOffsetField.text = HorizontalOffset.ToString(CultureInfo.InvariantCulture);
            VerticalOffsetField.text = VerticalOffset.ToString(CultureInfo.InvariantCulture);
            ReverseCheckbox.isChecked = IsReversed;
            if (!IsCurrentItem)
            {
                var index = ParallelRoadTool.AvailableRoadTypes.FindIndex(ni => ni != null && ni.name == NetInfo.name);
                DebugUtils.Log($"selecting index {index}");
                DropDown.selectedIndex = index;
                size = new Vector2(size.x, size.y * 2 + 4);
                return;
            }

            DeleteButton.isVisible = false;
            HorizontalOffsetField.isVisible = false;
            VerticalOffsetField.isVisible = false;
            ReverseCheckbox.isVisible = false;
            DropDown.isVisible = false;
            DropdownFilterField.isVisible = false;
            Label.isVisible = true;
            AddButton.isVisible = true;
            IsFiltered = false;
            Label.text = Locale.Get("PRT_TEXTS", "SameAsSelectedLabel");
        }

        private void DropDown_eventSelectedIndexChanged(UIComponent component, int index)
        {
            DebugUtils.Log("UINetTypeItem.DropDown_eventChanged");
            OnChangedCallback?.Invoke();
        }

        private void HorizontalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            if (!float.TryParse(value, out HorizontalOffset)) return;
            OnChangedCallback?.Invoke();
        }

        private void VerticalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            if (!float.TryParse(value, out VerticalOffset)) return;
            OnChangedCallback?.Invoke();
        }

        private void ReverseCheckboxOnEventCheckChanged(UIComponent component, bool value)
        {
            OnChangedCallback?.Invoke();
        }

        private void AddButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DebugUtils.Log("UINetTypeItem.AddButton_eventClicked");
            OnAddCallback?.Invoke();
        }

        private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DebugUtils.Log("UINetTypeItem.DeleteButton_eventClicked");
            OnDeleteCallback?.Invoke();
        }

        private void DropdownFilterField_eventTextChanged(UIComponent component, string value)
        {
            DebugUtils.Log($"Searching for {value} ...");
            FilterText = value;
            PopulateDropDown();
            DropDown.selectedIndex = 0;
            DropDown_eventSelectedIndexChanged(DropDown, 0);
            DebugUtils.Log($"Found {DropDown.items.Length} items");
        }
    }
}