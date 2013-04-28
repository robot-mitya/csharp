// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameRobot.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Класс управления приложением.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.GamerServices;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Media;

    using RoboCommon;

    /// <summary>
    /// Состояние приложения: главное меню или управление роботом.
    /// </summary>
    internal enum GameState 
    {
        /// <summary>
        /// Режим отображения меню.
        /// </summary>
        gsMenu,

        /// <summary>
        /// Режим управления роботом.
        /// </summary>
        gsRobotControl 
    }

    /// <summary>
    /// Способ управления роботом.
    /// </summary>
    internal enum ControlType
    {
        /// <summary>
        /// Управление клавиатурой.
        /// </summary>
        ctKeyboard,

        /// <summary>
        /// Управление геймпэдом.
        /// </summary>
        ctGamepad
    }
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameRobot : Microsoft.Xna.Framework.Game
    {
        /// <summary>
        /// Settings helper.
        /// </summary>
        private ControlSettingsHelper controlSettingsHelper;

        /// <summary>
        /// Объект для взаимодействия с роботом.
        /// </summary>
        private UdpCommunicationHelper communicationHelper;

        /// <summary>
        /// Объект для работы с фарами робота.
        /// </summary>
        private FlashlightHelper flashlightHelper;

        /// <summary>
        /// Объект для работы с ходовыми двигателями робота.
        /// </summary>
        private DriveHelper driveHelper;

        /// <summary>
        /// Объект для работы с сервоприводами головы робота.
        /// </summary>
        private LookHelper lookHelper;

        /// <summary>
        /// Объект для установки эмоций робота.
        /// </summary>
        private MoodHelper moodHelper;

        /// <summary>
        /// Объект для работы с ИК-пушкой робота.
        /// </summary>
        private GunHelper gunHelper;

        /// <summary>
        /// Объект для приёма и воспроизведения видеопотока.
        /// </summary>
        private VideoHelper videoHelper;

        /// <summary>
        /// Объект для приёма и воспроизведения аудиопотока.
        /// </summary>
        private AudioHelper audioHelper;

        /// <summary>
        /// Менеджер графического устройства.
        /// </summary>
        private GraphicsDeviceManager graphics;

        /// <summary>
        /// Признак необходимости переключить режим отображения с полного экрана на окно или наоборот.
        /// </summary>
        private bool toggleFullScreen;

        /// <summary>
        /// Область для рисования.
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// Текстура для вывода видео.
        /// </summary>
        private Texture2D videoTexture;

        /// <summary>
        /// Шрифт для вывода текста.
        /// </summary>
        private SpriteFont debugFont;

        /// <summary>
        /// Текущее состояние приложения.
        /// </summary>
        private GameState gameState = GameState.gsMenu;

        /// <summary>
        /// Последний момент времени, когда отправлялись команды управления двигателями и поворотом головы.
        /// </summary>
        private DateTime lastTimeCommandSent = new DateTime();

        /// <summary>
        /// Предыдущее состояние геймпэда.
        /// </summary>
        private GamePadState previousGamePadState;

        /// <summary>
        /// Предыдущее состояние клавиатуры.
        /// </summary>
        private KeyboardState previousKeyboardState;

        /// <summary>
        /// Способ управления роботом: клавиатура или геймпэд.
        /// </summary>
        private ControlType controlType;

        /// <summary>
        /// Initializes a new instance of the GameRobot class.
        /// </summary>
        public GameRobot()
        {
            this.IsFixedTimeStep = false;
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.SynchronizeWithVerticalRetrace = false;
            
            // this.graphics.IsFullScreen = true;
            // this.IsMouseVisible = false;
            Content.RootDirectory = "Content";

            this.controlSettingsHelper = new ControlSettingsHelper();
            this.controlSettingsHelper.Load();

            this.communicationHelper = new UdpCommunicationHelper(
                this.controlSettingsHelper.Settings.RoboHeadAddress,
                this.controlSettingsHelper.Settings.UdpSendPort,
                this.controlSettingsHelper.Settings.UdpReceivePort,
                this.controlSettingsHelper.Settings.SingleMessageRepetitionsCount);

            this.flashlightHelper = new FlashlightHelper(this.communicationHelper);
            this.driveHelper = new DriveHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
            this.lookHelper = new LookHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
            this.moodHelper = new MoodHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
            this.gunHelper = new GunHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
            this.videoHelper = new VideoHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
            this.audioHelper = new AudioHelper(this.communicationHelper, this.controlSettingsHelper.Settings);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.graphics.IsFullScreen = true;

            base.Initialize();

            this.previousGamePadState = GamePad.GetState(PlayerIndex.One);
            this.previousKeyboardState = Keyboard.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Деинициализация управления роботом.
        /// </summary>
        protected override void EndRun()
        {
            base.EndRun();

            if (this.gameState == GameState.gsRobotControl)
            {
                this.FinalizeRobot();
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            this.debugFont = Content.Load<SpriteFont>("CourierNew");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!this.IsActive)
            {
                return;
            }

            KeyboardState keyboardState = Keyboard.GetState(PlayerIndex.One);
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            bool shiftIsPressed = this.IsShiftPressed(keyboardState);
            bool shiftIsNotPressed = !shiftIsPressed;
            bool ctrlIsPressed = this.IsCtrlPressed(keyboardState);
            bool ctrlIsNotPressed = !ctrlIsPressed;
            bool altIsPressed = this.IsAltPressed(keyboardState);
            bool altIsNotPressed = !altIsPressed;
            bool nothingIsPressed = shiftIsNotPressed && ctrlIsNotPressed && altIsNotPressed;
            bool onlyAltIsPressed = shiftIsNotPressed && altIsPressed && ctrlIsNotPressed;

            // Переключение режима отображения: полный экран / окно.
            if (this.IsKeyChangedToDown(keyboardState, Keys.Enter) && onlyAltIsPressed)
            {
                this.toggleFullScreen = true;
            }

            // Запуск режима управления роботом клавиатурой.
            if (this.IsKeyChangedToDown(keyboardState, Keys.Space) && nothingIsPressed)
            {
                this.controlType = ControlType.ctKeyboard;
                this.gameState = GameState.gsRobotControl;
                this.InitializeRobot();
            }

            // Запуск режима управления роботом геймпэдом.
            if (this.IsButtonChangedToDown(gamePadState, Buttons.Start))
            {
                this.controlType = ControlType.ctGamepad;
                this.gameState = GameState.gsRobotControl;
                this.InitializeRobot();
            }

            if (this.IsButtonPressed(gamePadState, gamePadState.Buttons.Back))
            {
                this.Exit();
            }

            if (this.gameState == GameState.gsRobotControl)
            {
                this.CommonUpdateInRobotControlState(gameTime, keyboardState);

                switch (this.controlType)
                {
                    case ControlType.ctKeyboard:
                        this.KeyboardUpdateInRobotControlState(gameTime, keyboardState);
                        break;
                    case ControlType.ctGamepad:
                        this.GamepadUpdateInRobotControlState(gameTime, gamePadState);
                        break;
                }
                
                this.videoTexture = this.videoHelper.GetVideoTexture(this.GraphicsDevice);
            }

            this.previousGamePadState = gamePadState;
            this.previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Переключение режима отображения: полный экран / окно.
            if (this.toggleFullScreen)
            {
                this.graphics.ToggleFullScreen();
                this.toggleFullScreen = false;
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (this.gameState == GameState.gsMenu)
            {
                this.DrawInMenuState(gameTime);
            }
            else if (this.gameState == GameState.gsRobotControl)
            {
                this.DrawInRobotControlState(gameTime);
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Обнаружение нажатия на кнопку геймпэда.
        /// </summary>
        /// <param name="gamePadState">Текущее состояние геймпэда.</param>
        /// <param name="button">Отслеживаемая кнопка.</param>
        /// <returns>true, если кнопка была нажата.</returns>
        private bool IsButtonChangedToDown(GamePadState gamePadState, Buttons button)
        {
            if (gamePadState.IsConnected == false)
            {
                return false;
            }

            return gamePadState.IsButtonDown(button) && (this.previousGamePadState.IsButtonDown(button) == false);
        }

        /// <summary>
        /// Обнаружение отпускания кнопки геймпэда.
        /// </summary>
        /// <param name="gamePadState">Текущее состояние геймпэда.</param>
        /// <param name="button">Отслеживаемая кнопка.</param>
        /// <returns>true, если кнопка была отпущена.</returns>
        private bool IsButtonChangedToUp(GamePadState gamePadState, Buttons button)
        {
            if (gamePadState.IsConnected == false)
            {
                return false;
            }

            return (gamePadState.IsButtonDown(button) == false) && this.previousGamePadState.IsButtonDown(button);
        }

        /// <summary>
        /// Проверка зажата ли кнопка геймпэда.
        /// </summary>
        /// <param name="gamePadState">Текущее состояние геймпэда.</param>
        /// <param name="buttonState">Отслеживаемая кнопка.</param>
        /// <returns>true, если кнопка зажата.</returns>
        private bool IsButtonPressed(GamePadState gamePadState, ButtonState buttonState)
        {
            if (gamePadState.IsConnected == false)
            {
                return false;
            }

            return buttonState == ButtonState.Pressed;
        }

        /// <summary>
        /// Обнаружение нажатия на клавишу клавиатуры.
        /// </summary>
        /// <param name="keyboardState">Текущее состояние клавиатуры.</param>
        /// <param name="key">Отслеживаемая клавиша.</param>
        /// <returns>true, если клавиша была нажата.</returns>
        private bool IsKeyChangedToDown(KeyboardState keyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key) && (this.previousKeyboardState.IsKeyDown(key) == false);
        }

        /// <summary>
        /// Обнаружение отпускания клавиши клавиатуры.
        /// </summary>
        /// <param name="keyboardState">Текущее состояние клавиатуры.</param>
        /// <param name="key">Отслеживаемая клавиша.</param>
        /// <returns>true, если клавиша была отпущена.</returns>
        private bool IsKeyChangedToUp(KeyboardState keyboardState, Keys key)
        {
            return (keyboardState.IsKeyDown(key) == false) && this.previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Проверка зажата ли кнопка клавиатуры.
        /// </summary>
        /// <param name="keyboardState">Текущее состояние клавиатуры.</param>
        /// <param name="key">Отслеживаемая кнопка.</param>
        /// <returns>true, если кнопка зажата.</returns>
        private bool IsKeyPressed(KeyboardState keyboardState, Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Update в обоих режимах: управления роботом посредством клавиатуры и джойстика.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        /// <param name="keyboardState">Состояние клавиатуры.</param>
        private void CommonUpdateInRobotControlState(GameTime gameTime, KeyboardState keyboardState)
        {
            bool shiftIsPressed = this.IsShiftPressed(keyboardState);
            bool shiftIsNotPressed = !shiftIsPressed;
            bool ctrlIsPressed = this.IsCtrlPressed(keyboardState);
            bool ctrlIsNotPressed = !ctrlIsPressed;
            bool altIsPressed = this.IsAltPressed(keyboardState);
            bool altIsNotPressed = !altIsPressed;
            bool nothingIsPressed = shiftIsNotPressed && ctrlIsNotPressed && altIsNotPressed;
            bool onlyCtrlIsPressed = shiftIsNotPressed && altIsNotPressed && ctrlIsPressed;

            if (this.IsKeyChangedToDown(keyboardState, Keys.F1) && nothingIsPressed)
            {
                this.moodHelper.SetMood(Mood.Normal);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F2) && nothingIsPressed)
            {
                this.moodHelper.SetMood(Mood.Happy);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F2) && onlyCtrlIsPressed)
            {
                this.moodHelper.ShowReadyToPlay(this.lookHelper);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F3) && nothingIsPressed)
            {
                this.moodHelper.SetMood(Mood.Blue);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F3) && onlyCtrlIsPressed)
            {
                this.moodHelper.ShowDepression(this.lookHelper);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F4) && nothingIsPressed)
            {
                this.moodHelper.SetMood(Mood.Disaster);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.F5) && nothingIsPressed)
            {
                this.moodHelper.SetMood(Mood.Angry);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.T) && nothingIsPressed)
            {
                this.moodHelper.WagTail();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Y) && nothingIsPressed)
            {
                this.moodHelper.ShowYes();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.N) && nothingIsPressed)
            {
                this.moodHelper.ShowNo();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D0) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript0);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D1) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript1);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D2) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript2);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D3) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript3);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D4) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript4);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D5) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript5);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D6) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript6);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D7) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript7);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D8) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript8);
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D9) && onlyCtrlIsPressed)
            {
                this.PlayRoboScript(this.controlSettingsHelper.Settings.RoboScript9);
            }
        }

        /// <summary>
        /// Выполнение РобоСкрипта после нажатия на горячую клавишу.
        /// </summary>
        /// <param name="roboScriptItem">
        /// Объект-РобоСкрипт, загруженный из опций.
        /// </param>
        private void PlayRoboScript(RoboScriptItem roboScriptItem)
        {
            if (!roboScriptItem.IsEmpty)
            {
                if (!roboScriptItem.WasSent)
                {
                    this.communicationHelper.SendRoboScriptToRobot(roboScriptItem.RoboScript);
                    roboScriptItem.WasSent = true;
                }

                this.communicationHelper.SendMessageToRobot(roboScriptItem.PlayCommand);
            }
        }

        /// <summary>
        /// Update в режиме управления роботом посредством клавиатуры.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        /// <param name="keyboardState">Состояние клавиатуры.</param>
        private void KeyboardUpdateInRobotControlState(GameTime gameTime, KeyboardState keyboardState)
        {
            bool shiftIsPressed = this.IsShiftPressed(keyboardState);
            bool shiftIsNotPressed = !shiftIsPressed;
            bool ctrlIsPressed = this.IsCtrlPressed(keyboardState);
            bool ctrlIsNotPressed = !ctrlIsPressed;
            bool altIsPressed = this.IsAltPressed(keyboardState);
            bool altIsNotPressed = !altIsPressed;
            bool nothingIsPressed = shiftIsNotPressed && ctrlIsNotPressed && altIsNotPressed;
            bool onlyCtrlIsPressed = shiftIsNotPressed && altIsNotPressed && ctrlIsPressed;

            if (this.IsKeyChangedToDown(keyboardState, Keys.L) && nothingIsPressed)
            {
                this.flashlightHelper.Switch();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Tab) && nothingIsPressed)
            {
                this.gunHelper.Fire();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Scroll) && nothingIsPressed)
            {
                this.driveHelper.SwitchTurboMode();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.PageUp) && nothingIsPressed)
            {
                // В прогулочном режиме центральное направление взгляда по вертикали - строго горизонтально.
                // Так проще целиться управлять движением. В режиме общения (не прогулочный) - чуть вверх.
                this.lookHelper.WalkModeOn = false;
                this.lookHelper.LookForward();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.PageDown) && nothingIsPressed)
            {
                // В прогулочном режиме центральное направление взгляда по вертикали - строго горизонтально.
                // Так проще целиться управлять движением. В режиме общения (не прогулочный) - чуть вверх.
                this.lookHelper.WalkModeOn = true;
                this.lookHelper.LookForward();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Home) && nothingIsPressed)
            {
                // Установка головы в положение "смотреть вперёд".
                this.lookHelper.LookForward();
            }

            // Установка признака быстрого фиксированного обзора (при отпускании кнопок управления
            // голова остаётся в установленном положении). При быстром фиксированном обзоре 
            // голова поворачивается с большей скоростью.
            this.lookHelper.FastModeOn = shiftIsPressed;

            if (this.IsKeyChangedToDown(keyboardState, Keys.Left) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Поворот головы влево с фиксацией. Угол поворота определяется значением gameTime.
                this.lookHelper.StartLeftTurn();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Right) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Поворот головы вправо с фиксацией. Угол поворота определяется значением gameTime.
                this.lookHelper.StartRightTurn();
            }

            if (this.IsKeyChangedToUp(keyboardState, Keys.Left) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Остановка поворота головы влево с фиксацией.
                this.lookHelper.StopHorizontalTurn();
            }

            if (this.IsKeyChangedToUp(keyboardState, Keys.Right) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Остановка поворота головы вправо с фиксацией.
                this.lookHelper.StopHorizontalTurn();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Up) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Поворот головы вверх с фиксацией.
                this.lookHelper.StartUpTurn();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.Down) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Поворот головы вниз с фиксацией.
                this.lookHelper.StartDownTurn();
            }

            if (this.IsKeyChangedToUp(keyboardState, Keys.Up) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Остановка поворота головы вверх с фиксацией.
                this.lookHelper.StopVerticalTurn();
            }

            if (this.IsKeyChangedToUp(keyboardState, Keys.Down) && ctrlIsNotPressed && altIsNotPressed)
            {
                // Остановка поворота головы вниз с фиксацией.
                this.lookHelper.StopVerticalTurn();
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D1) && nothingIsPressed)
            {
                this.driveHelper.SpeedForKeyboardControl = this.controlSettingsHelper.Settings.Speed1;
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D2) && nothingIsPressed)
            {
                this.driveHelper.SpeedForKeyboardControl = this.controlSettingsHelper.Settings.Speed2;
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D3) && nothingIsPressed)
            {
                this.driveHelper.SpeedForKeyboardControl = this.controlSettingsHelper.Settings.Speed3;
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D4) && nothingIsPressed)
            {
                this.driveHelper.SpeedForKeyboardControl = this.controlSettingsHelper.Settings.Speed4;
            }

            if (this.IsKeyChangedToDown(keyboardState, Keys.D5) && nothingIsPressed)
            {
                this.driveHelper.SpeedForKeyboardControl = this.controlSettingsHelper.Settings.Speed5;
            }

            DateTime nowTime = DateTime.Now;
            TimeSpan timePassed = nowTime - this.lastTimeCommandSent;
            if (timePassed >= this.controlSettingsHelper.Settings.MinCommandInterval)
            {
                this.driveHelper.Drive(
                    this.IsKeyPressed(keyboardState, Keys.W), 
                    this.IsKeyPressed(keyboardState, Keys.S),
                    this.IsKeyPressed(keyboardState, Keys.A), 
                    this.IsKeyPressed(keyboardState, Keys.D));

                this.lastTimeCommandSent = nowTime;
            }
        }

        /// <summary>
        /// Update в режиме управления роботом посредством геймпэда.
        /// </summary>
        /// <param name="gameTime">Игровое время.</param>
        /// <param name="gamePadState">Состояние геймпэда.</param>
        private void GamepadUpdateInRobotControlState(GameTime gameTime, GamePadState gamePadState)
        {
            if (this.IsButtonChangedToDown(gamePadState, Buttons.A))
            {
                this.moodHelper.ShowYes();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.B))
            {
                this.moodHelper.ShowNo();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.X))
            {
                this.moodHelper.WagTail();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.Y))
            {
                this.flashlightHelper.Switch();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.RightTrigger))
            {
                this.flashlightHelper.TurnOn();
            }

            if (this.IsButtonChangedToUp(gamePadState, Buttons.RightTrigger))
            {
                this.flashlightHelper.TurnOff();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.LeftTrigger))
            {
                this.gunHelper.Fire();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.LeftStick))
            {
                this.driveHelper.SwitchTurboMode();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.RightStick))
            {
                // В прогулочном режиме центральное направление взгляда по вертикали - строго горизонтально.
                // Так проще целиться управлять движением. В режиме общения (не прогулочный) - чуть вверх.
                this.lookHelper.WalkModeOn = !this.lookHelper.WalkModeOn;
                this.lookHelper.LookForward();
            }

            // Установка признака быстрого фиксированного обзора (при отпускании кнопок управления
            // голова остаётся в установленном положении). При быстром фиксированном обзоре 
            // голова поворачивается с большей скоростью.
            this.lookHelper.FastModeOn = this.IsButtonPressed(gamePadState, gamePadState.Buttons.RightShoulder);

            if (this.IsButtonChangedToDown(gamePadState, Buttons.DPadLeft))
            {
                // Поворот головы влево с постоянной скоростью.
                this.lookHelper.StartLeftTurn();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.DPadRight))
            {
                // Поворот головы вправо с постоянной скоростью.
                this.lookHelper.StartRightTurn();
            }

            if (this.IsButtonChangedToUp(gamePadState, Buttons.DPadLeft) || 
                this.IsButtonChangedToUp(gamePadState, Buttons.DPadRight))
            {
                // Останов поворота головы в горизонтальной плоскости.
                this.lookHelper.StopHorizontalTurn();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.DPadUp))
            {
                // Поворот головы вверх с постоянной скоростью.
                this.lookHelper.StartUpTurn();
            }

            if (this.IsButtonChangedToDown(gamePadState, Buttons.DPadDown))
            {
                // Поворот головы вниз с постоянной скоростью.
                this.lookHelper.StartDownTurn();
            }

            if (this.IsButtonChangedToUp(gamePadState, Buttons.DPadUp) ||
                this.IsButtonChangedToUp(gamePadState, Buttons.DPadDown))
            {
                // Останов поворота головы в вертикальной плоскости.
                this.lookHelper.StopVerticalTurn();
            }

            // Скорости двигателей и углы сервоприводов головы определяются и устанавливаются с заданной периодичностью.
            if (gamePadState.IsConnected)
            {
                DateTime nowTime = DateTime.Now;
                TimeSpan timePassed = nowTime - this.lastTimeCommandSent;
                if (timePassed >= this.controlSettingsHelper.Settings.MinCommandInterval)
                {
                    this.driveHelper.RotationModeOn = gamePadState.Buttons.LeftShoulder == ButtonState.Pressed;
                    this.driveHelper.Drive(gamePadState.ThumbSticks.Left.X, gamePadState.ThumbSticks.Left.Y);

                    this.lookHelper.Look(gamePadState.ThumbSticks.Right.X, gamePadState.ThumbSticks.Right.Y);

                    this.lastTimeCommandSent = nowTime;
                }
            }
        }

        /// <summary>
        /// Draws when the program is in menu mode.
        /// </summary>
        /// <param name="gameTime">
        /// Current game time.
        /// </param>
        private void DrawInMenuState(GameTime gameTime)
        {
            this.spriteBatch.Begin();

            if (this.IsActive && (this.videoTexture != null))
            {
                Rectangle rectangle = new Rectangle(
                    0,
                    0,
                    this.graphics.PreferredBackBufferWidth,
                    this.graphics.PreferredBackBufferHeight);
                this.spriteBatch.Draw(this.videoTexture, rectangle, Color.White);
            }

            Color color = Color.White;
            this.spriteBatch.DrawString(this.debugFont, "Для управления клавиатурой нажмите Пробел", ScreenConstants.GetTextPosition(0, 0), color);
            this.spriteBatch.DrawString(this.debugFont, "Для управления геймпэдом XBOX 360 нажмите Start", ScreenConstants.GetTextPosition(0, 1), color);

            color = Color.Yellow;
            if (this.controlSettingsHelper != null)
            {
                this.spriteBatch.DrawString(this.debugFont, "v" + this.controlSettingsHelper.GetProductVersion(), ScreenConstants.GetTextPosition(0, 4), color);
            }

            this.spriteBatch.End();
        }

        /// <summary>
        /// Отрисовка экрана в режиме управления роботом.
        /// </summary>
        /// <param name="gameTime">
        /// Игровое время.
        /// </param>
        private void DrawInRobotControlState(GameTime gameTime)
        {
            this.spriteBatch.Begin();

            if (this.IsActive && (this.videoTexture != null))
            {
                Rectangle rectangle = new Rectangle(
                    0,
                    0,
                    this.graphics.PreferredBackBufferWidth,
                    this.graphics.PreferredBackBufferHeight);
                this.spriteBatch.Draw(this.videoTexture, rectangle, Color.White);
            }

            Color color;
            string motorCommand;
            string speedText = " (max " + Math.Round((double)this.driveHelper.SpeedForKeyboardControl * 100 / 255).ToString() + "%)";

            motorCommand = this.driveHelper.LeftMotorCommand + speedText;
            color = Color.White;
            this.spriteBatch.DrawString(this.debugFont, motorCommand, ScreenConstants.GetTextPosition(0, 0), color);

            motorCommand = this.driveHelper.RightMotorCommand;
            color = Color.White;
            this.spriteBatch.DrawString(this.debugFont, motorCommand, ScreenConstants.GetTextPosition(0, 1), color);

            if (this.driveHelper.TurboModeOn)
            {
                this.spriteBatch.DrawString(this.debugFont, "Турбо режим", ScreenConstants.GetTextPosition(1, 0), Color.Orange);
            }

            if (this.driveHelper.RotationModeOn)
            {
                this.spriteBatch.DrawString(this.debugFont, "Режим разворота", ScreenConstants.GetTextPosition(1, 1), Color.Orange);
            }

            if (this.lookHelper.FastModeOn)
            {
                this.spriteBatch.DrawString(this.debugFont, "Быстрый обзор", ScreenConstants.GetTextPosition(1, 2), Color.Orange);
            }

            if (!this.lookHelper.WalkModeOn)
            {
                this.spriteBatch.DrawString(this.debugFont, "Режим обзора", ScreenConstants.GetTextPosition(1, 3), Color.Orange);
            }

            this.spriteBatch.DrawString(this.debugFont, this.lookHelper.HorizontalServoCommand, ScreenConstants.GetTextPosition(0, 2), Color.White);
            this.spriteBatch.DrawString(this.debugFont, this.lookHelper.VerticalServoCommand, ScreenConstants.GetTextPosition(0, 3), Color.White);

            color = this.flashlightHelper.FlashlightTurnedOn ? Color.Yellow : Color.White;
            this.spriteBatch.DrawString(this.debugFont, this.flashlightHelper.FlashlightCommand, ScreenConstants.GetTextPosition(0, 4), color);

            string moodText = "Настроение: ";
            switch (this.moodHelper.Mood)
            {
                case Mood.Happy:
                    moodText += "счастлив";
                    break;
                case Mood.Blue:
                    moodText += "грустно";
                    break;
                case Mood.Angry:
                    moodText += "злой";
                    break;
                case Mood.Disaster:
                    moodText += "раздавлен";
                    break;
                default:
                    moodText += "нормально";
                    break;
            }

            this.spriteBatch.DrawString(this.debugFont, moodText, ScreenConstants.GetTextPosition(0, 5), Color.White);

            this.spriteBatch.DrawString(this.debugFont, this.PercentToText(this.gunHelper.GetChargePercent()), ScreenConstants.GetTextPosition(0, 6), Color.White);

            this.spriteBatch.DrawString(this.debugFont, this.communicationHelper.LastErrorMessage, ScreenConstants.GetTextPosition(0, 7), Color.Orange);

            this.spriteBatch.End();
        }

        /// <summary>
        /// Получение строки для отображения степени "заряда" пушки для выстрела.
        /// Время "заряда" пушки определяется в Settings.GunChargeTime.
        /// </summary>
        /// <param name="percent">Величина "заряда" в процентах.</param>
        /// <returns>Строка вида ++++++---</returns>
        private string PercentToText(int percent)
        {
            const int PercentStep = 5;

            string result = string.Empty;
            for (int i = 0; i < 100 / PercentStep; i++)
            {
                int endPercent = (i + 1) * PercentStep;
                if (endPercent <= percent)
                {
                    result += "+";
                }
                else
                {
                    result += "-";
                }
            }

            return result;
        }

        /// <summary>
        /// Инициализация робота.
        /// </summary>
        private void InitializeRobot()
        {
            this.driveHelper.Stop();
            this.lookHelper.LookForward();
            this.flashlightHelper.TurnOff();

            // Запуск воспроизведения видео:
            this.videoHelper.InitializeVideo();

            // Запуск воспроизведения аудио:
            this.audioHelper.InitializeAudio();
        }
        
        /// <summary>
        /// Деинициализация робота.
        /// </summary>
        private void FinalizeRobot()
        {
            this.driveHelper.Stop();
            this.lookHelper.LookForward();
            this.flashlightHelper.TurnOff();
            Thread.Sleep(1000); // (немного ждем прихода последних эхо-команд от Android-приложения)

            this.videoHelper.FinalizeVideo();
            this.audioHelper.FinalizeAudio();
        }

        /// <summary>
        /// Определяет нажата ли хоть одна клавиша Shift.
        /// </summary>
        /// <param name="keyboardState">Состояние клавиатуры.</param>
        /// <returns>True, если нажата клавиша Shift.</returns>
        private bool IsShiftPressed(KeyboardState keyboardState)
        {
            return this.IsKeyPressed(keyboardState, Keys.LeftShift) || this.IsKeyPressed(keyboardState, Keys.RightShift);
        }

        /// <summary>
        /// Определяет нажата ли хоть одна клавиша Ctrl.
        /// </summary>
        /// <param name="keyboardState">Состояние клавиатуры.</param>
        /// <returns>True, если нажата клавиша Ctrl.</returns>
        private bool IsCtrlPressed(KeyboardState keyboardState)
        {
            return this.IsKeyPressed(keyboardState, Keys.LeftControl) || this.IsKeyPressed(keyboardState, Keys.RightControl);
        }

        /// <summary>
        /// Определяет нажата ли хоть одна клавиша Alt.
        /// </summary>
        /// <param name="keyboardState">Состояние клавиатуры.</param>
        /// <returns>True, если нажата клавиша Alt.</returns>
        private bool IsAltPressed(KeyboardState keyboardState)
        {
            return this.IsKeyPressed(keyboardState, Keys.LeftAlt) || this.IsKeyPressed(keyboardState, Keys.RightAlt);
        }

        /*
        /// <summary>
        /// Чтение епций приложения из файла конфигурации.
        /// </summary>
        private void LoadControlSettingsFromFile()
        {
            this.controlSettingsHelper.ReverseHeadTangage = Properties.Settings.Default.ReverseHeadTangage;
            this.controlSettingsHelper.IpWebcamPort = Properties.Settings.Default.IpWebcamPort;
            this.controlSettingsHelper.DriveModeNormalMaxSpeed = Properties.Settings.Default.DriveModeNormalMaxSpeed;
            this.controlSettingsHelper.DriveModeTurboMaxSpeed = Properties.Settings.Default.DriveModeTurboMaxSpeed;
            this.controlSettingsHelper.Speed1 = Properties.Settings.Default.Speed1;
            this.controlSettingsHelper.Speed2 = Properties.Settings.Default.Speed2;
            this.controlSettingsHelper.Speed3 = Properties.Settings.Default.Speed3;
            this.controlSettingsHelper.Speed4 = Properties.Settings.Default.Speed4;
            this.controlSettingsHelper.Speed5 = Properties.Settings.Default.Speed5;

            this.controlSettingsHelper.SlowHeadTurnPeriod = Properties.Settings.Default.SlowHeadTurnPeriod;
            this.controlSettingsHelper.FastHeadTurnPeriod = Properties.Settings.Default.FastHeadTurnPeriod;

            this.controlSettingsHelper.VerticalMinimumDegree1 = Properties.Settings.Default.VerticalMinimumDegree1;
            this.controlSettingsHelper.VerticalForwardDegree1 = Properties.Settings.Default.VerticalForwardDegree1;
            this.controlSettingsHelper.VerticalMaximumDegree1 = Properties.Settings.Default.VerticalMaximumDegree1;
            this.controlSettingsHelper.VerticalMinimumDegree2 = Properties.Settings.Default.VerticalMinimumDegree2;
            this.controlSettingsHelper.VerticalForwardDegree2 = Properties.Settings.Default.VerticalForwardDegree2;
            this.controlSettingsHelper.VerticalMaximumDegree2 = Properties.Settings.Default.VerticalMaximumDegree2;

            this.controlSettingsHelper.PlayVideo = Properties.Settings.Default.PlayVideo;
            this.controlSettingsHelper.PlayAudio = Properties.Settings.Default.PlayAudio;

            this.controlSettingsHelper.RoboScripts[0].Initialize(Properties.Settings.Default.RoboScript0);
            this.controlSettingsHelper.RoboScripts[1].Initialize(Properties.Settings.Default.RoboScript1);
            this.controlSettingsHelper.RoboScripts[2].Initialize(Properties.Settings.Default.RoboScript2);
            this.controlSettingsHelper.RoboScripts[3].Initialize(Properties.Settings.Default.RoboScript3);
            this.controlSettingsHelper.RoboScripts[4].Initialize(Properties.Settings.Default.RoboScript4);
            this.controlSettingsHelper.RoboScripts[5].Initialize(Properties.Settings.Default.RoboScript5);
            this.controlSettingsHelper.RoboScripts[6].Initialize(Properties.Settings.Default.RoboScript6);
            this.controlSettingsHelper.RoboScripts[7].Initialize(Properties.Settings.Default.RoboScript7);
            this.controlSettingsHelper.RoboScripts[8].Initialize(Properties.Settings.Default.RoboScript8);
            this.controlSettingsHelper.RoboScripts[9].Initialize(Properties.Settings.Default.RoboScript9);
        }*/
    }
}
