using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace TerraIntegration.UI
{
	internal class UIFocusInputTextField : UIPanel
	{
		public bool UnfocusOnTab { get; internal set; }

		public event Action OnTextChange;
		public event Action OnUnfocus;
		public event Action OnTab;

		public UIFocusInputTextField(string hintText)
		{
			_hintText = hintText;

			PaddingTop = 3;
			PaddingLeft = 3;
			PaddingRight = 3;
			PaddingBottom = 0;

		}

		public void SetText(string text)
		{
			if (text == null)
			{
				text = "";
			}
			if (CurrentString != text)
			{
				CurrentString = text;
                OnTextChange?.Invoke();
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			Main.clrInput();
			Focused = true;
			Main.blockInput = true;
		}

		// Token: 0x06002225 RID: 8741 RVA: 0x004AF880 File Offset: 0x004ADA80
		public override void Update(GameTime gameTime)
		{
			if (!ContainsPoint(Main.MouseScreen) && Main.mouseLeft)
			{
				Focused = false;
				Main.blockInput = false;
				OnUnfocus?.Invoke();
			}
			base.Update(gameTime);
		}

		// Token: 0x06002226 RID: 8742 RVA: 0x004AF8D5 File Offset: 0x004ADAD5
		private static bool JustPressed(Keys key)
		{
			return Main.inputText.IsKeyDown(key) && !Main.oldInputText.IsKeyDown(key);
		}

		// Token: 0x06002227 RID: 8743 RVA: 0x004AF8F4 File Offset: 0x004ADAF4
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (Focused)
			{
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newString = Main.GetInputText(CurrentString, false);
				if (!newString.Equals(CurrentString))
				{
					CurrentString = newString;
                    OnTextChange?.Invoke();
				}
				else
				{
					CurrentString = newString;
				}
				if (JustPressed(Keys.Tab))
				{
					if (UnfocusOnTab)
					{
						Focused = false;
						Main.blockInput = false;
						OnUnfocus?.Invoke();
					}
                    OnTab?.Invoke();
				}
				int num = _textBlinkerCount + 1;
				_textBlinkerCount = num;
				if (num >= 20)
				{
					_textBlinkerState = (_textBlinkerState + 1) % 2;
					_textBlinkerCount = 0;
				}
			}
			string displayString = CurrentString;
			if (_textBlinkerState == 1 && Focused)
			{
				displayString += "|";
			}
			CalculatedStyle space = GetDimensions();
			space.X += PaddingLeft;
			space.Y += PaddingTop;
			if (CurrentString.Length == 0 && !Focused)
			{
				Utils.DrawBorderString(spriteBatch, _hintText, new Vector2(space.X, space.Y), Color.Gray, 1f, 0f, 0f, -1);
				return;
			}
			Utils.DrawBorderString(spriteBatch, displayString, new Vector2(space.X, space.Y), Color.White, 1f, 0f, 0f, -1);
		}
		internal bool Focused;

		public string CurrentString { get; private set; } = "";

		private readonly string _hintText;
		private int _textBlinkerCount;
		private int _textBlinkerState;

	}
}
