using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerraIntegration.UI
{
	public class UIFocusInputTextField : UIPanel
	{
		public bool UnfocusOnTab { get; internal set; }

		public delegate string ModifyTextInputDelegate(string newText, string oldText);

		public ModifyTextInputDelegate ModifyTextInput;

		public event Action OnTextChange;
		public event Action OnUnfocus;
		public event Action OnTab;

		public StyleDimension TextHAlign = default;
		public StyleDimension TextVAlign = new(0, .5f);

		internal bool Focused;

		public string CurrentString { get; private set; } = "";

		private readonly string _hintText;
		private int _textBlinkerCount;
		private int _textBlinkerState;

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
			if (ModifyTextInput is not null)
				text = ModifyTextInput(text, CurrentString);
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
					if (ModifyTextInput is not null)
						newString = ModifyTextInput(newString, CurrentString);

					if (!newString.Equals(CurrentString))
					{
						CurrentString = newString;
						OnTextChange?.Invoke();
					}
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

			if (CurrentString.Length == 0 && !Focused)
			{

				DrawAlignedText(spriteBatch, _hintText, Color.Gray, false);
				return;
			}
			DrawAlignedText(spriteBatch, CurrentString, Color.White, Focused && _textBlinkerState == 1);
		}

		void DrawAlignedText(SpriteBatch spriteBatch, string text, Color color, bool blinker)
		{
			CalculatedStyle space = GetDimensions();

			space.X += PaddingLeft;
			space.Y += PaddingTop;

			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 size = font.MeasureString(text);

			if (size.Y <= 0)
				size.Y = font.LineSpacing;

			Vector2 pos = space.Position();
			pos.X += TextHAlign.GetValue(space.Width - size.X);
			pos.Y += TextHAlign.GetValue(space.Height - size.Y);

			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, pos, color, 0f, Vector2.Zero, Vector2.One);

			if (blinker)
			{
				pos.X += size.X;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, "|", pos, color, 0f, Vector2.Zero, Vector2.One);
			}

			
		}
	}

	
}
