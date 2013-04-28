// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlSettings.cs" company="Dzakhov's jag">
//   Copyright © Dmitry Dzakhov 2011
// </copyright>
// <summary>
//   Класс, хранящий настройки приложения для управления роботом.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboControl
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Класс, хранящий настройки приложения.
    /// </summary>
    /// <remarks>
    /// Класс должен взаимодействовать с файлом конфигурации приложения.
    /// </remarks>
    public class ControlSettings
    {
        /// <summary>
        /// Initializes a new instance of the ControlSettings class.
        /// </summary>
        public ControlSettings()
        {
            this.RoboHeadAddress = "192.168.1.1";
            this.UdpSendPort = 51974;
            this.UdpReceivePort = 51973;
            this.SingleMessageRepetitionsCount = 3;

            this.SinAlphaBound = 0.06;

            this.HorizontalMinimumDegree = 0;
            this.HorizontalForwardDegree = 90;
            this.HorizontalMaximumDegree = 180;

            this.VerticalMinimumDegree1 = 0;
            this.VerticalForwardDegree1 = 30;
            this.VerticalMaximumDegree1 = 60;

            this.VerticalMinimumDegree2 = 0;
            this.VerticalForwardDegree2 = 45;
            this.VerticalMaximumDegree2 = 90;

            this.VerticalMinimumDegree = this.VerticalMinimumDegree1;
            this.VerticalForwardDegree = this.VerticalForwardDegree1;
            this.VerticalMaximumDegree = this.VerticalMaximumDegree1;

            this.VerticalReadyToPlayDegree = 50;

            this.MinCommandInterval = new TimeSpan(0, 0, 0, 0, 20);

            this.GunChargeTime = new TimeSpan(0, 0, 5);

            // Значения по умолчанию для параметров, переопределяемых в конфигурационном файле:
            this.ReverseHeadTangage = false;
            this.IpWebcamPort = 8080;
            this.DriveModeNormalMaxSpeed = 255;
            this.DriveModeTurboMaxSpeed = 255;
            this.Speed1 = 51;
            this.Speed2 = 102;
            this.Speed3 = 153;
            this.Speed4 = 204;
            this.Speed5 = 255;
            this.SlowHeadTurnPeriod = 1000;
            this.FastHeadTurnPeriod = 170;

            this.PlayAudio = false;
            this.PlayVideo = true;

            this.RoboScript0 = new RoboScriptItem(0);
            this.RoboScript0.SetRoboScript("r0100, Z0001, I0000, W0000, Z0000");

            this.RoboScript1 = new RoboScriptItem(1);
            this.RoboScript1.SetRoboScript("r0101, Z0001, M0105, W0000, Z0000");

            this.RoboScript2 = new RoboScriptItem(2);
            this.RoboScript2.SetRoboScript("r0102, Z000A, L00FF, W0000, R0050, W03E8, LFFB0, W0000, RFF01, W03E8, L00FF, W0000, R0050, W03E8, L00FF, W0000, RFF01, W03E8, L0000, W0000, R0000, W0000, Z0000");
            
            this.RoboScript3 = new RoboScriptItem(3);
            this.RoboScript3.SetRoboScript(string.Empty);
            
            this.RoboScript4 = new RoboScriptItem(4);
            this.RoboScript4.SetRoboScript(string.Empty);
            
            this.RoboScript5 = new RoboScriptItem(5);
            this.RoboScript5.SetRoboScript(string.Empty);
            
            this.RoboScript6 = new RoboScriptItem(6);
            this.RoboScript6.SetRoboScript(string.Empty);
            
            this.RoboScript7 = new RoboScriptItem(7);
            this.RoboScript7.SetRoboScript(string.Empty);
            
            this.RoboScript8 = new RoboScriptItem(8);
            this.RoboScript8.SetRoboScript(string.Empty);
            
            this.RoboScript9 = new RoboScriptItem(9);
            this.RoboScript9.SetRoboScript("r0109, Z0001, I0001, W0000, Z0000");
        }

        /// <summary>
        /// Gets or sets IP-address of the phone.
        /// </summary>
        public string RoboHeadAddress { get; set; }

        /// <summary>
        /// Gets or sets UDP port for datagram output.
        /// </summary>
        public int UdpSendPort { get; set; }

        /// <summary>
        /// Gets or sets UDP port for datagram input.
        /// </summary>
        public int UdpReceivePort { get; set; }
        
        /// <summary>
        /// Gets or sets quantity of send message repetitions.
        /// </summary>
        /// <remarks>
        /// We use UDP datagrams so single messages like I0001 can be lost. That's why we repeat sending for SingleMessageRepetitionsCount times. Three times by default.
        /// </remarks>
        public int SingleMessageRepetitionsCount { get; set; }

        /// <summary>
        /// Gets Граница для "малых" углов. Углы, синус которых превышает заданную веричину не считаются "малыми".
        /// Используется вместе с признаком режима разворота для разворота робота на месте.
        /// </summary>
        [XmlIgnore]
        public double SinAlphaBound { get; private set; }

        /// <summary>
        /// Gets Минимальный угол поворота сервопривода, управляющего горизонтальным поворотом головы.
        /// </summary>
        [XmlIgnore]
        public int HorizontalMinimumDegree { get; private set; }

        /// <summary>
        /// Gets Угол поворота сервопривода, управляющего горизонтальным поворотом головы, соответствующий центральной позиции.
        /// </summary>
        [XmlIgnore]
        public int HorizontalForwardDegree { get; private set; }
        
        /// <summary>
        /// Gets Максимальный угол поворота сервопривода, управляющего горизонтальным поворотом головы.
        /// </summary>
        [XmlIgnore]
        public int HorizontalMaximumDegree { get; private set; }

        /// <summary>
        /// Gets or sets Минимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (обычный режим).
        /// </summary>        
        public int VerticalMinimumDegree1 { get; set; }

        /// <summary>
        /// Gets or sets Угол поворота сервопривода, управляющего вертикальным поворотом головы, соответствующий 
        /// центральной позиции (обычный режим).
        /// </summary>
        public int VerticalForwardDegree1 { get; set; }

        /// <summary>
        /// Gets or sets Максимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (обычный режим).
        /// </summary>
        public int VerticalMaximumDegree1 { get; set; }

        /// <summary>
        /// Gets or sets Минимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (прогулочный режим).
        /// </summary>        
        public int VerticalMinimumDegree2 { get; set; }

        /// <summary>
        /// Gets or sets Угол поворота сервопривода, управляющего вертикальным поворотом головы, соответствующий 
        /// центральной позиции (прогулочный режим).
        /// </summary>
        public int VerticalForwardDegree2 { get; set; }

        /// <summary>
        /// Gets or sets Максимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (прогулочный режим).
        /// </summary>
        public int VerticalMaximumDegree2 { get; set; }

        /// <summary>
        /// Gets or sets Минимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (текущий режим).
        /// </summary>        
        [XmlIgnore]
        public int VerticalMinimumDegree { get; set; }
        
        /// <summary>
        /// Gets or sets Угол поворота сервопривода, управляющего вертикальным поворотом головы, соответствующий 
        /// центральной позиции (текущий режим).
        /// </summary>
        [XmlIgnore]
        public int VerticalForwardDegree { get; set; }
        
        /// <summary>
        /// Gets or sets Максимальный угол поворота сервопривода, управляющего вертикальным поворотом головы (текущий режим).
        /// </summary>
        [XmlIgnore]
        public int VerticalMaximumDegree { get; set; }

        /// <summary>
        /// Gets Угол поворота сервопривода, управляющего вертикальным поворотом головы для игры.
        /// </summary>
        [XmlIgnore]
        public int VerticalReadyToPlayDegree { get; private set; }

        /// <summary>
        /// Gets Значение, соответствующее высокой скорости горизонтального поворота головы.
        /// </summary>
        [XmlIgnore]
        [Obsolete("HorizontalHighSpeed is deprecated.")]
        public float HorizontalHighSpeed { get; private set; }

        /// <summary>
        /// Gets Значение, соответствующее низкой скорости горизонтального поворота головы.
        /// </summary>
        [XmlIgnore]
        [Obsolete("HorizontalLowSpeed is deprecated.")]
        public float HorizontalLowSpeed { get; private set; }

        /// <summary>
        /// Gets Значение, соответствующее высокой скорости вертикального поворота головы.
        /// </summary>
        [XmlIgnore]
        [Obsolete("VerticalHighSpeed is deprecated.")]
        public float VerticalHighSpeed { get; private set; }

        /// <summary>
        /// Gets Значение, соответствующее низкой скорости вертикального поворота головы.
        /// </summary>
        [XmlIgnore]
        [Obsolete("VerticalLowSpeed is deprecated.")]
        public float VerticalLowSpeed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether управление движением головы по Y-координате перевёрнуто.
        /// </summary>
        public bool ReverseHeadTangage { get; set; }

        /// <summary>
        /// Gets Период опроса джойстика движения и нефиксированного поворота головы.
        /// </summary>
        [XmlIgnore]
        public TimeSpan MinCommandInterval { get; private set; }

        /// <summary>
        /// Gets or sets Порт для связи с IP Webcam.
        /// </summary>
        public int IpWebcamPort { get; set; }

        /// <summary>
        /// Gets or sets Определяет скорость в нормальном (не турбо) режиме движения. 
        /// Окончательная скорость определяется умножением на коэффициент, равный DriveModeNormalMaxSpeed / 255.
        /// </summary>
        public byte DriveModeNormalMaxSpeed { get; set; }

        /// <summary>
        /// Gets or sets Определяет скорость в турбо-режиме движения. 
        /// Окончательная скорость определяется умножением на коэффициент, равный DriveModeTurboMaxSpeed / 255.
        /// </summary>
        public byte DriveModeTurboMaxSpeed { get; set; }

        /// <summary>
        /// Gets Время "заряда" пушки - минимальный временной интервал между выстрелами.
        /// </summary>
        [XmlIgnore]
        public TimeSpan GunChargeTime { get; private set; }

        /// <summary>
        /// Gets or sets 1-ая скорость при управлении от клавиатуры.
        /// </summary>
        public byte Speed1 { get; set; }

        /// <summary>
        /// Gets or sets 2-ая скорость при управлении от клавиатуры.
        /// </summary>
        public byte Speed2 { get; set; }

        /// <summary>
        /// Gets or sets 3-ья скорость при управлении от клавиатуры.
        /// </summary>
        public byte Speed3 { get; set; }

        /// <summary>
        /// Gets or sets 4-ая скорость при управлении от клавиатуры.
        /// </summary>
        public byte Speed4 { get; set; }

        /// <summary>
        /// Gets or sets 5-ая скорость при управлении от клавиатуры.
        /// </summary>
        public byte Speed5 { get; set; }

        /// <summary>
        /// Gets or sets slow head turn period in sentiseconds.
        /// </summary>
        public int SlowHeadTurnPeriod { get; set; }

        /// <summary>
        /// Gets or sets fast head turn period in sentiseconds.
        /// </summary>
        public int FastHeadTurnPeriod { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether видео воспроизводится.
        /// </summary>
        public bool PlayVideo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether звук воспроизводится.
        /// </summary>
        public bool PlayAudio { get; set; }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №0.
        /// </summary>
        public string RoboScriptText0 
        { 
            get
            {
                return this.RoboScript0.RoboScript;
            }
           
            set
            {
                this.RoboScript0.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №1.
        /// </summary>
        public string RoboScriptText1
        { 
            get
            {
                return this.RoboScript1.RoboScript;
            }
           
            set
            {
                this.RoboScript1.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №2.
        /// </summary>
        public string RoboScriptText2
        { 
            get
            {
                return this.RoboScript2.RoboScript;
            }
           
            set
            {
                this.RoboScript2.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №3.
        /// </summary>
        public string RoboScriptText3
        { 
            get
            {
                return this.RoboScript3.RoboScript;
            }
           
            set
            {
                this.RoboScript3.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №4.
        /// </summary>
        public string RoboScriptText4
        { 
            get
            {
                return this.RoboScript4.RoboScript;
            }
           
            set
            {
                this.RoboScript4.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №5.
        /// </summary>
        public string RoboScriptText5
        { 
            get
            {
                return this.RoboScript5.RoboScript;
            }
           
            set
            {
                this.RoboScript5.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №6.
        /// </summary>
        public string RoboScriptText6
        { 
            get
            {
                return this.RoboScript6.RoboScript;
            }
           
            set
            {
                this.RoboScript6.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №7.
        /// </summary>
        public string RoboScriptText7
        { 
            get
            {
                return this.RoboScript7.RoboScript;
            }
           
            set
            {
                this.RoboScript7.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №8.
        /// </summary>
        public string RoboScriptText8
        { 
            get
            {
                return this.RoboScript8.RoboScript;
            }
           
            set
            {
                this.RoboScript8.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets or sets Текст РобоСкрипта №9.
        /// </summary>
        public string RoboScriptText9
        { 
            get
            {
                return this.RoboScript9.RoboScript;
            }
           
            set
            {
                this.RoboScript9.SetRoboScript(value);
            }
        }

        /// <summary>
        /// Gets РобоСкрипт №0.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript0 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №1.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript1 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №2.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript2 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №3.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript3 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №4.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript4 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №5.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript5 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №6.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript6 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №7.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript7 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №8.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript8 { get; private set; }

        /// <summary>
        /// Gets РобоСкрипт №9.
        /// </summary>
        [XmlIgnore]
        public RoboScriptItem RoboScript9 { get; private set; }
    }
}
