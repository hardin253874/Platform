// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace CodeCoverageUtil
{
    using System;
    using System.ComponentModel;
    using System.Management;
    using System.Collections;
    using System.Globalization;
    
    
    /// <summary>
    /// 
    /// </summary>
    internal class Service : System.ComponentModel.Component {
        
        // Private property to hold the WMI namespace in which the class resides.
        private static string CreatedWmiNamespace = "root\\cimv2";
        
        // Private property to hold the name of WMI class which created this class.
        private static string CreatedClassName = "Win32_Service";
        
        // Private member variable to hold the ManagementScope which is used by the various methods.
        private static System.Management.ManagementScope statMgmtScope;
        
        private ManagementSystemProperties PrivateSystemProperties;
        
        // Underlying lateBound WMI object.
        private System.Management.ManagementObject PrivateLateBoundObject;
        
        // Member variable to store the 'automatic commit' behavior for the class.
        private bool AutoCommitProp;
        
        // Private variable to hold the embedded property representing the instance.
        private System.Management.ManagementBaseObject embeddedObj;
        
        // The current WMI object used
        private System.Management.ManagementBaseObject curObj;
        
        // Flag to indicate if the instance is an embedded object.
        private bool isEmbedded;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        public Service() {
            this.InitializeObject(null, null, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        public Service(string keyName) {
            this.InitializeObject(null, new System.Management.ManagementPath(Service.ConstructPath(keyName)), null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="keyName">Name of the key.</param>
        public Service(System.Management.ManagementScope mgmtScope, string keyName) {
            this.InitializeObject(((System.Management.ManagementScope)(mgmtScope)), new System.Management.ManagementPath(Service.ConstructPath(keyName)), null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="getOptions">The get options.</param>
        public Service(System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            this.InitializeObject(null, path, getOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="path">The path.</param>
        public Service(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path) {
            this.InitializeObject(mgmtScope, path, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public Service(System.Management.ManagementPath path) {
            this.InitializeObject(null, path, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="path">The path.</param>
        /// <param name="getOptions">The get options.</param>
        public Service(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            this.InitializeObject(mgmtScope, path, getOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="theObject">The object.</param>
        public Service(System.Management.ManagementObject theObject) {
            Initialize();
            if ((CheckIfProperClass(theObject) == true)) {
                PrivateLateBoundObject = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
                curObj = PrivateLateBoundObject;
            }
            else {
                throw new System.ArgumentException("Class name does not match.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="theObject">The object.</param>
        public Service(System.Management.ManagementBaseObject theObject) {
            Initialize();
            if ((CheckIfProperClass(theObject) == true)) {
                embeddedObj = theObject;
                PrivateSystemProperties = new ManagementSystemProperties(theObject);
                curObj = embeddedObj;
                isEmbedded = true;
            }
            else {
                throw new System.ArgumentException("Class name does not match.");
            }
        }
        
        // Property returns the namespace of the WMI class.
        /// <summary>
        /// Gets the originating namespace.
        /// </summary>
        /// <value>The originating namespace.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string OriginatingNamespace {
            get {
                return "root\\cimv2";
            }
        }

        /// <summary>
        /// Gets the name of the management class.
        /// </summary>
        /// <value>The name of the management class.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ManagementClassName {
            get {
                string strRet = CreatedClassName;
                if ((curObj != null)) {
                    if ((curObj.ClassPath != null)) {
                        strRet = ((string)(curObj["__CLASS"]));
                        if (string.IsNullOrEmpty(strRet)) {
                            strRet = CreatedClassName;
                        }
                    }
                }
                return strRet;
            }
        }
        
        // Property pointing to an embedded object to get System properties of the WMI object.
        /// <summary>
        /// Gets the system properties.
        /// </summary>
        /// <value>The system properties.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ManagementSystemProperties SystemProperties {
            get {
                return PrivateSystemProperties;
            }
        }
        
        // Property returning the underlying lateBound object.
        /// <summary>
        /// Gets the late bound object.
        /// </summary>
        /// <value>The late bound object.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementBaseObject LateBoundObject {
            get {
                return curObj;
            }
        }
        
        // ManagementScope of the object.
        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>The scope.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Management.ManagementScope Scope {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Scope;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    PrivateLateBoundObject.Scope = value;
                }
            }
        }
        
        // Property to show the commit behavior for the WMI object. If true, WMI object will be automatically saved after each property modification.(ie. Put() is called after modification of a property).
        /// <summary>
        /// Gets or sets a value indicating whether [auto commit].
        /// </summary>
        /// <value><c>true</c> if [auto commit]; otherwise, <c>false</c>.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoCommit {
            get {
                return AutoCommitProp;
            }
            set {
                AutoCommitProp = value;
            }
        }
        
        // The ManagementPath of the underlying WMI object.
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [Browsable(true)]
        public System.Management.ManagementPath Path {
            get {
                if ((isEmbedded == false)) {
                    return PrivateLateBoundObject.Path;
                }
                else {
                    return null;
                }
            }
            set {
                if ((isEmbedded == false)) {
                    if ((CheckIfProperClass(null, value, null) != true)) {
                        throw new System.ArgumentException("Class name does not match.");
                    }
                    PrivateLateBoundObject.Path = value;
                }
            }
        }
        
        // Public static scope property which is used by the various methods.
        /// <summary>
        /// Gets or sets the static scope.
        /// </summary>
        /// <value>The static scope.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static System.Management.ManagementScope StaticScope {
            get {
                return statMgmtScope;
            }
            set {
                statMgmtScope = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is accept pause null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is accept pause null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAcceptPauseNull {
            get {
                if ((curObj["AcceptPause"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [accept pause].
        /// </summary>
        /// <value><c>true</c> if [accept pause]; otherwise, <c>false</c>.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The AcceptPause property indicates whether the service can be paused.\nValues: TRU" +
            "E or FALSE. A value of TRUE indicates the service can be paused.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool AcceptPause {
            get {
                if ((curObj["AcceptPause"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["AcceptPause"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is accept stop null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is accept stop null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsAcceptStopNull {
            get {
                if ((curObj["AcceptStop"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [accept stop].
        /// </summary>
        /// <value><c>true</c> if [accept stop]; otherwise, <c>false</c>.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The AcceptStop property indicates whether the service can be stopped.\nValues: TRU" +
            "E or FALSE. A value of TRUE indicates the service can be stopped.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool AcceptStop {
            get {
                if ((curObj["AcceptStop"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["AcceptStop"]));
            }
        }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>The caption.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Caption property is a short textual description (one-line string) of the obje" +
            "ct.")]
        public string Caption {
            get {
                return ((string)(curObj["Caption"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is check point null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is check point null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCheckPointNull {
            get {
                if ((curObj["CheckPoint"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the check point.
        /// </summary>
        /// <value>The check point.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The CheckPoint property specifies a value that the service increments periodically to report its progress during a lengthy start, stop, pause, or continue operation. For example, the service should increment this value as it completes each step of its initialization when it is starting up. The user interface program that invoked the operation on the service uses this value to track the progress of the service during a lengthy operation. This value is not valid and should be zero when the service does not have a start, stop, pause, or continue operation pending.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint CheckPoint {
            get {
                if ((curObj["CheckPoint"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["CheckPoint"]));
            }
        }

        /// <summary>
        /// Gets the name of the creation class.
        /// </summary>
        /// <value>The name of the creation class.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("CreationClassName indicates the name of the class or the subclass used in the cre" +
            "ation of an instance. When used with the other key properties of this class, thi" +
            "s property allows all instances of this class and its subclasses to be uniquely " +
            "identified.")]
        public string CreationClassName {
            get {
                return ((string)(curObj["CreationClassName"]));
            }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Description property provides a textual description of the object. ")]
        public string Description {
            get {
                return ((string)(curObj["Description"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is desktop interact null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is desktop interact null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDesktopInteractNull {
            get {
                if ((curObj["DesktopInteract"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [desktop interact].
        /// </summary>
        /// <value><c>true</c> if [desktop interact]; otherwise, <c>false</c>.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The DesktopInteract property indicates whether the service can create or communic" +
            "ate with windows on the desktop.\nValues: TRUE or FALSE. A value of TRUE indicate" +
            "s the service can create or communicate with windows on the desktop.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool DesktopInteract {
            get {
                if ((curObj["DesktopInteract"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["DesktopInteract"]));
            }
        }

        /// <summary>
        /// Gets the name of the display.
        /// </summary>
        /// <value>The name of the display.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The DisplayName property indicates the display name of the service. This string has a maximum length of 256 characters. The name is case-preserved in the Service Control Manager. DisplayName comparisons are always case-insensitive. 
Constraints: Accepts the same value as the Name property.
Example: Atdisk.")]
        public string DisplayName {
            get {
                return ((string)(curObj["DisplayName"]));
            }
        }

        /// <summary>
        /// Gets the error control.
        /// </summary>
        /// <value>The error control.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"If this service fails to start during startup, the ErrorControl property specifies the severity of the error. The value indicates the action taken by the startup program if failure occurs. All errors are logged by the computer system. The computer system does not notify the user of ""Ignore"" errors. With ""Normal"" errors the user is notified. With ""Severe"" errors, the system is restarted with the last-known-good configuration. Finally, on""Critical"" errors the system attempts to restart with a good configuration.")]
        public string ErrorControl {
            get {
                return ((string)(curObj["ErrorControl"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is exit code null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is exit code null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExitCodeNull {
            get {
                if ((curObj["ExitCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>The exit code.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ExitCode property specifies a Win32 error code defining any problems encountered in starting or stopping the service. This property is set to ERROR_SERVICE_SPECIFIC_ERROR (1066) when the error is unique to the service represented by this class, and information about the error is available in the ServiceSpecificExitCode member. The service sets this value to NO_ERROR when running, and again upon normal termination.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint ExitCode {
            get {
                if ((curObj["ExitCode"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["ExitCode"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is install date null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is install date null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInstallDateNull {
            get {
                if ((curObj["InstallDate"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the install date.
        /// </summary>
        /// <value>The install date.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The InstallDate property is datetime value indicating when the object was install" +
            "ed. A lack of a value does not indicate that the object is not installed.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public System.DateTime InstallDate {
            get {
                if ((curObj["InstallDate"] != null)) {
                    return ToDateTime(((string)(curObj["InstallDate"])));
                }
                else {
                    return System.DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The Name property uniquely identifies the service and provides an indication of t" +
            "he functionality that is managed. This functionality is described in more detail" +
            " in the object\'s Description property. ")]
        public string Name {
            get {
                return ((string)(curObj["Name"]));
            }
        }

        /// <summary>
        /// Gets the name of the path.
        /// </summary>
        /// <value>The name of the path.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The PathName property contains the fully qualified path to the service binary fil" +
            "e that implements the service.\nExample: \\SystemRoot\\System32\\drivers\\afd.sys")]
        public string PathName {
            get {
                return ((string)(curObj["PathName"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is process id null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is process id null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsProcessIdNull {
            get {
                if ((curObj["ProcessId"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the process id.
        /// </summary>
        /// <value>The process id.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ProcessId property specifies the process identifier of the service.\nExample: " +
            "324")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint ProcessId {
            get {
                if ((curObj["ProcessId"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["ProcessId"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is service specific exit code null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is service specific exit code null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsServiceSpecificExitCodeNull {
            get {
                if ((curObj["ServiceSpecificExitCode"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the service specific exit code.
        /// </summary>
        /// <value>The service specific exit code.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The ServiceSpecificExitCode property specifies a service-specific error code for errors that occur while the service is either starting or stopping. The exit codes are defined by the service represented by this class. This value is only set when the ExitCodeproperty value is ERROR_SERVICE_SPECIFIC_ERROR, 1066.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint ServiceSpecificExitCode {
            get {
                if ((curObj["ServiceSpecificExitCode"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["ServiceSpecificExitCode"]));
            }
        }

        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>The type of the service.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The ServiceType property supplies the type of service provided to calling process" +
            "es.")]
        public string ServiceType {
            get {
                return ((string)(curObj["ServiceType"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is started null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is started null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsStartedNull {
            get {
                if ((curObj["Started"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Service"/> is started.
        /// </summary>
        /// <value><c>true</c> if started; otherwise, <c>false</c>.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("Started is a boolean indicating whether the service has been started (TRUE), or s" +
            "topped (FALSE).")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public bool Started {
            get {
                if ((curObj["Started"] == null)) {
                    return System.Convert.ToBoolean(0);
                }
                return ((bool)(curObj["Started"]));
            }
        }

        /// <summary>
        /// Gets the start mode.
        /// </summary>
        /// <value>The start mode.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The StartMode property indicates the start mode of the Win32 base service. ""Boot"" specifies a device driver started by the operating system loader. This value is valid only for driver services. ""System"" specifies a device driver started by the IoInitSystem function. This value is valid only for driver services. ""Automatic"" specifies a service to be started automatically by the service control manager during system startup. ""Manual"" specifies a service to be started by the service control manager when a process calls the StartService function. ""Disabled"" specifies a service that can no longer be started.")]
        public string StartMode {
            get {
                return ((string)(curObj["StartMode"]));
            }
        }

        /// <summary>
        /// Gets the start name.
        /// </summary>
        /// <value>The start name.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The StartName property indicates the account name under which the service runs. Depending on the service type, the account name may be in the form of ""DomainName\Username"".  The service process will be logged using one of these two forms when it runs. If the account belongs to the built-in domain, "".\Username"" can be specified. If NULL is specified, the service will be logged on as the LocalSystem account. For kernel or system level drivers, StartName contains the driver object name (that is, \FileSystem\Rdr or \Driver\Xns) which the input and output (I/O) system uses to load the device driver. Additionally, if NULL is specified, the driver runs with a default object name created by the I/O system based on the service name.
Example: DWDOM\Admin.")]
        public string StartName {
            get {
                return ((string)(curObj["StartName"]));
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The State property indicates the current state of the base service.")]
        public string State {
            get {
                return ((string)(curObj["State"]));
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states.")]
        public string Status {
            get {
                return ((string)(curObj["Status"]));
            }
        }

        /// <summary>
        /// Gets the name of the system creation class.
        /// </summary>
        /// <value>The name of the system creation class.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The scoping System\'s CreationClassName. ")]
        public string SystemCreationClassName {
            get {
                return ((string)(curObj["SystemCreationClassName"]));
            }
        }

        /// <summary>
        /// Gets the name of the system.
        /// </summary>
        /// <value>The name of the system.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description("The name of the system that hosts this service")]
        public string SystemName {
            get {
                return ((string)(curObj["SystemName"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is tag id null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is tag id null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTagIdNull {
            get {
                if ((curObj["TagId"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the tag id.
        /// </summary>
        /// <value>The tag id.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The TagId property specifies a unique tag value for this service in the group. A value of 0 indicates that the service has not been assigned a tag. A tag can be used for ordering service startup within a load order group by specifying a tag order vector in the registry located at: HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\GroupOrderList. Tags are only evaluated for Kernel Driver and File System Driver start type services that have ""Boot"" or ""System"" start modes.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint TagId {
            get {
                if ((curObj["TagId"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["TagId"]));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is wait hint null.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is wait hint null; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsWaitHintNull {
            get {
                if ((curObj["WaitHint"] == null)) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the wait hint.
        /// </summary>
        /// <value>The wait hint.</value>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Description(@"The WaitHint property specifies the estimated time required (in milliseconds) for a pending start, stop, pause, or continue operation. After the specified amount of time has elapsed, the service makes its next call to the SetServiceStatus function with either an incremented CheckPoint value or a change in Current State. If the amount of time specified by WaitHint passes, and CheckPoint has not been incremented, or the Current State has not changed, the service control manager or service control program assumes that an error has occurred.")]
        [TypeConverter(typeof(WMIValueTypeConverter))]
        public uint WaitHint {
            get {
                if ((curObj["WaitHint"] == null)) {
                    return System.Convert.ToUInt32(0);
                }
                return ((uint)(curObj["WaitHint"]));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions OptionsParam) {
            if (((path != null) 
                        && (string.Compare(path.ClassName, this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            else {
                return CheckIfProperClass(new System.Management.ManagementObject(mgmtScope, path, OptionsParam));
            }
        }
        
        private bool CheckIfProperClass(System.Management.ManagementBaseObject theObj) {
            if (((theObj != null) 
                        && (string.Compare(((string)(theObj["__CLASS"])), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0))) {
                return true;
            }
            else {
                System.Array parentClasses = ((System.Array)(theObj["__DERIVATION"]));
                if ((parentClasses != null)) {
                    int count = 0;
                    for (count = 0; (count < parentClasses.Length); count = (count + 1)) {
                        if ((string.Compare(((string)(parentClasses.GetValue(count))), this.ManagementClassName, true, System.Globalization.CultureInfo.InvariantCulture) == 0)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private bool ShouldSerializeAcceptPause() {
            if ((this.IsAcceptPauseNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeAcceptStop() {
            if ((this.IsAcceptStopNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeCheckPoint() {
            if ((this.IsCheckPointNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeDesktopInteract() {
            if ((this.IsDesktopInteractNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeExitCode() {
            if ((this.IsExitCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        // Converts a given datetime in DMTF format to System.DateTime object.
        static System.DateTime ToDateTime(string dmtfDate) {
            System.DateTime initializer = System.DateTime.MinValue;
            int year = initializer.Year;
            int month = initializer.Month;
            int day = initializer.Day;
            int hour = initializer.Hour;
            int minute = initializer.Minute;
            int second = initializer.Second;
            long ticks = 0;
            string dmtf = dmtfDate;
            System.DateTime datetime = System.DateTime.MinValue;
            string tempString = string.Empty;
            if ((dmtf == null)) {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length == 0)) {
                throw new System.ArgumentOutOfRangeException();
            }
            if ((dmtf.Length != 25)) {
                throw new System.ArgumentOutOfRangeException();
            }
            try {
                tempString = dmtf.Substring(0, 4);
                if (("****" != tempString)) {
                    year = int.Parse(tempString);
                }
                tempString = dmtf.Substring(4, 2);
                if (("**" != tempString)) {
                    month = int.Parse(tempString);
                }
                tempString = dmtf.Substring(6, 2);
                if (("**" != tempString)) {
                    day = int.Parse(tempString);
                }
                tempString = dmtf.Substring(8, 2);
                if (("**" != tempString)) {
                    hour = int.Parse(tempString);
                }
                tempString = dmtf.Substring(10, 2);
                if (("**" != tempString)) {
                    minute = int.Parse(tempString);
                }
                tempString = dmtf.Substring(12, 2);
                if (("**" != tempString)) {
                    second = int.Parse(tempString);
                }
                tempString = dmtf.Substring(15, 6);
                if (("******" != tempString)) {
                    ticks = (long.Parse(tempString) * ((long)((System.TimeSpan.TicksPerMillisecond / 1000))));
                }
                if (((((((((year < 0) 
                            || (month < 0)) 
                            || (day < 0)) 
                            || (hour < 0)) 
                            || (minute < 0)) 
                            || (minute < 0)) 
                            || (second < 0)) 
                            || (ticks < 0))) {
                    throw new System.ArgumentOutOfRangeException();
                }
            }
            catch (System.Exception e) {
                throw new System.ArgumentOutOfRangeException(null, e.Message);
            }
            datetime = new System.DateTime(year, month, day, hour, minute, second, 0);
            datetime = datetime.AddTicks(ticks);
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(datetime);
            int UTCOffset = 0;
            int OffsetToBeAdjusted = 0;
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            tempString = dmtf.Substring(22, 3);
            if ((tempString != "******")) {
                tempString = dmtf.Substring(21, 4);
                try {
                    UTCOffset = int.Parse(tempString);
                }
                catch (System.Exception e) {
                    throw new System.ArgumentOutOfRangeException(null, e.Message);
                }
                OffsetToBeAdjusted = ((int)((OffsetMins - UTCOffset)));
                datetime = datetime.AddMinutes(((double)(OffsetToBeAdjusted)));
            }
            return datetime;
        }
        
        // Converts a given System.DateTime object to DMTF datetime format.
        static string ToDmtfDateTime(System.DateTime date) {
            string utcString = string.Empty;
            System.TimeSpan tickOffset = System.TimeZone.CurrentTimeZone.GetUtcOffset(date);
            long OffsetMins = ((long)((tickOffset.Ticks / System.TimeSpan.TicksPerMinute)));
            if ((System.Math.Abs(OffsetMins) > 999)) {
                date = date.ToUniversalTime();
                utcString = "+000";
            }
            else {
                if ((tickOffset.Ticks >= 0)) {
                    utcString = string.Concat("+", ((System.Int64 )((tickOffset.Ticks / System.TimeSpan.TicksPerMinute))).ToString().PadLeft(3, '0'));
                }
                else {
                    string strTemp = ((System.Int64 )(OffsetMins)).ToString();
                    utcString = string.Concat("-", strTemp.Substring(1, (strTemp.Length - 1)).PadLeft(3, '0'));
                }
            }
            string dmtfDateTime = ((System.Int32 )(date.Year)).ToString().PadLeft(4, '0');
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32 )(date.Month)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32 )(date.Day)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32 )(date.Hour)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32 )(date.Minute)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ((System.Int32 )(date.Second)).ToString().PadLeft(2, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, ".");
            System.DateTime dtTemp = new System.DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
            long microsec = ((long)((((date.Ticks - dtTemp.Ticks) 
                        * 1000) 
                        / System.TimeSpan.TicksPerMillisecond)));
            string strMicrosec = ((System.Int64 )(microsec)).ToString();
            if ((strMicrosec.Length > 6)) {
                strMicrosec = strMicrosec.Substring(0, 6);
            }
            dmtfDateTime = string.Concat(dmtfDateTime, strMicrosec.PadLeft(6, '0'));
            dmtfDateTime = string.Concat(dmtfDateTime, utcString);
            return dmtfDateTime;
        }
        
        private bool ShouldSerializeInstallDate() {
            if ((this.IsInstallDateNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeProcessId() {
            if ((this.IsProcessIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeServiceSpecificExitCode() {
            if ((this.IsServiceSpecificExitCodeNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeStarted() {
            if ((this.IsStartedNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeTagId() {
            if ((this.IsTagIdNull == false)) {
                return true;
            }
            return false;
        }
        
        private bool ShouldSerializeWaitHint() {
            if ((this.IsWaitHintNull == false)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Commits the object.
        /// </summary>
        [Browsable(true)]
        public void CommitObject() {
            if ((isEmbedded == false)) {
                PrivateLateBoundObject.Put();
            }
        }

        /// <summary>
        /// Commits the object.
        /// </summary>
        /// <param name="putOptions">The put options.</param>
        [Browsable(true)]
        public void CommitObject(System.Management.PutOptions putOptions) {
            if ((isEmbedded == false)) {
                PrivateLateBoundObject.Put(putOptions);
            }
        }
        
        private void Initialize() {
            AutoCommitProp = true;
            isEmbedded = false;
        }
        
        private static string ConstructPath(string keyName) {
            string strPath = "root\\cimv2:Win32_Service";
            strPath = string.Concat(strPath, string.Concat(".Name=", string.Concat("\"", string.Concat(keyName, "\""))));
            return strPath;
        }
        
        private void InitializeObject(System.Management.ManagementScope mgmtScope, System.Management.ManagementPath path, System.Management.ObjectGetOptions getOptions) {
            Initialize();
            if ((path != null)) {
                if ((CheckIfProperClass(mgmtScope, path, getOptions) != true)) {
                    throw new System.ArgumentException("Class name does not match.");
                }
            }
            PrivateLateBoundObject = new System.Management.ManagementObject(mgmtScope, path, getOptions);
            PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
            curObj = PrivateLateBoundObject;
        }
        
        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <returns></returns>
        public static ServiceCollection GetInstances() {
            return GetInstances(null, null, null);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(string condition) {
            return GetInstances(null, condition, null);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="selectedProperties">The selected properties.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(System.String [] selectedProperties) {
            return GetInstances(null, null, selectedProperties);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <param name="selectedProperties">The selected properties.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(string condition, System.String [] selectedProperties) {
            return GetInstances(null, condition, selectedProperties);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="enumOptions">The enum options.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.Management.EnumerationOptions enumOptions) {
            if ((mgmtScope == null)) {
                if ((statMgmtScope == null)) {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementPath pathObj = new System.Management.ManagementPath();
            pathObj.ClassName = "Win32_Service";
            pathObj.NamespacePath = "root\\cimv2";
            System.Management.ManagementClass clsObject = new System.Management.ManagementClass(mgmtScope, pathObj, null);
            if ((enumOptions == null)) {
                enumOptions = new System.Management.EnumerationOptions();
                enumOptions.EnsureLocatable = true;
            }
            return new ServiceCollection(clsObject.GetInstances(enumOptions));
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition) {
            return GetInstances(mgmtScope, condition, null);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="selectedProperties">The selected properties.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, System.String [] selectedProperties) {
            return GetInstances(mgmtScope, null, selectedProperties);
        }

        /// <summary>
        /// Gets the instances.
        /// </summary>
        /// <param name="mgmtScope">The MGMT scope.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="selectedProperties">The selected properties.</param>
        /// <returns></returns>
        public static ServiceCollection GetInstances(System.Management.ManagementScope mgmtScope, string condition, System.String [] selectedProperties) {
            if ((mgmtScope == null)) {
                if ((statMgmtScope == null)) {
                    mgmtScope = new System.Management.ManagementScope();
                    mgmtScope.Path.NamespacePath = "root\\cimv2";
                }
                else {
                    mgmtScope = statMgmtScope;
                }
            }
            System.Management.ManagementObjectSearcher ObjectSearcher = new System.Management.ManagementObjectSearcher(mgmtScope, new SelectQuery("Win32_Service", condition, selectedProperties));
            System.Management.EnumerationOptions enumOptions = new System.Management.EnumerationOptions();
            enumOptions.EnsureLocatable = true;
            ObjectSearcher.Options = enumOptions;
            return new ServiceCollection(ObjectSearcher.Get());
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <returns></returns>
        [Browsable(true)]
        public static Service CreateInstance() {
            System.Management.ManagementScope mgmtScope = null;
            if ((statMgmtScope == null)) {
                mgmtScope = new System.Management.ManagementScope();
                mgmtScope.Path.NamespacePath = CreatedWmiNamespace;
            }
            else {
                mgmtScope = statMgmtScope;
            }
            System.Management.ManagementPath mgmtPath = new System.Management.ManagementPath(CreatedClassName);
            System.Management.ManagementClass tmpMgmtClass = new System.Management.ManagementClass(mgmtScope, mgmtPath, null);
            return new Service(tmpMgmtClass.CreateInstance());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        [Browsable(true)]
        public void Delete() {
            PrivateLateBoundObject.Delete();
        }

        /// <summary>
        /// Changes the specified desktop interact.
        /// </summary>
        /// <param name="DesktopInteract">if set to <c>true</c> [desktop interact].</param>
        /// <param name="DisplayName">Name of the display.</param>
        /// <param name="ErrorControl">The error control.</param>
        /// <param name="LoadOrderGroup">The load order group.</param>
        /// <param name="LoadOrderGroupDependencies">The load order group dependencies.</param>
        /// <param name="PathName">Name of the path.</param>
        /// <param name="ServiceDependencies">The service dependencies.</param>
        /// <param name="ServiceType">Type of the service.</param>
        /// <param name="StartMode">The start mode.</param>
        /// <param name="StartName">The start name.</param>
        /// <param name="StartPassword">The start password.</param>
        /// <returns></returns>
        public uint Change(bool DesktopInteract, string DisplayName, byte ErrorControl, string LoadOrderGroup, string[] LoadOrderGroupDependencies, string PathName, string[] ServiceDependencies, byte ServiceType, string StartMode, string StartName, string StartPassword) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("Change");
                inParams["DesktopInteract"] = ((System.Boolean )(DesktopInteract));
                inParams["DisplayName"] = ((System.String )(DisplayName));
                inParams["ErrorControl"] = ((System.Byte )(ErrorControl));
                inParams["LoadOrderGroup"] = ((System.String )(LoadOrderGroup));
                inParams["LoadOrderGroupDependencies"] = ((string[])(LoadOrderGroupDependencies));
                inParams["PathName"] = ((System.String )(PathName));
                inParams["ServiceDependencies"] = ((string[])(ServiceDependencies));
                inParams["ServiceType"] = ((System.Byte )(ServiceType));
                inParams["StartMode"] = ((System.String )(StartMode));
                inParams["StartName"] = ((System.String )(StartName));
                inParams["StartPassword"] = ((System.String )(StartPassword));
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Change", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Changes the start mode.
        /// </summary>
        /// <param name="StartMode">The start mode.</param>
        /// <returns></returns>
        public uint ChangeStartMode(string StartMode) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("ChangeStartMode");
                inParams["StartMode"] = ((System.String )(StartMode));
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ChangeStartMode", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Creates the specified desktop interact.
        /// </summary>
        /// <param name="DesktopInteract">if set to <c>true</c> [desktop interact].</param>
        /// <param name="DisplayName">Name of the display.</param>
        /// <param name="ErrorControl">The error control.</param>
        /// <param name="LoadOrderGroup">The load order group.</param>
        /// <param name="LoadOrderGroupDependencies">The load order group dependencies.</param>
        /// <param name="Name">The name.</param>
        /// <param name="PathName">Name of the path.</param>
        /// <param name="ServiceDependencies">The service dependencies.</param>
        /// <param name="ServiceType">Type of the service.</param>
        /// <param name="StartMode">The start mode.</param>
        /// <param name="StartName">The start name.</param>
        /// <param name="StartPassword">The start password.</param>
        /// <returns></returns>
        public uint Create(bool DesktopInteract, string DisplayName, byte ErrorControl, string LoadOrderGroup, string[] LoadOrderGroupDependencies, string Name, string PathName, string[] ServiceDependencies, byte ServiceType, string StartMode, string StartName, string StartPassword) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("Create");
                inParams["DesktopInteract"] = ((System.Boolean )(DesktopInteract));
                inParams["DisplayName"] = ((System.String )(DisplayName));
                inParams["ErrorControl"] = ((System.Byte )(ErrorControl));
                inParams["LoadOrderGroup"] = ((System.String )(LoadOrderGroup));
                inParams["LoadOrderGroupDependencies"] = ((string[])(LoadOrderGroupDependencies));
                inParams["Name"] = ((System.String )(Name));
                inParams["PathName"] = ((System.String )(PathName));
                inParams["ServiceDependencies"] = ((string[])(ServiceDependencies));
                inParams["ServiceType"] = ((System.Byte )(ServiceType));
                inParams["StartMode"] = ((System.String )(StartMode));
                inParams["StartName"] = ((System.String )(StartName));
                inParams["StartPassword"] = ((System.String )(StartPassword));
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Create", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Delete0s this instance.
        /// </summary>
        /// <returns></returns>
        public uint Delete0() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("Delete", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Interrogates the service.
        /// </summary>
        /// <returns></returns>
        public uint InterrogateService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("InterrogateService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value, CultureInfo.InvariantCulture);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Pauses the service.
        /// </summary>
        /// <returns></returns>
        public uint PauseService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("PauseService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Resumes the service.
        /// </summary>
        /// <returns></returns>
        public uint ResumeService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("ResumeService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <returns></returns>
        public uint StartService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StartService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <returns></returns>
        public uint StopService() {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("StopService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }

        /// <summary>
        /// Users the control service.
        /// </summary>
        /// <param name="ControlCode">The control code.</param>
        /// <returns></returns>
        public uint UserControlService(byte ControlCode) {
            if ((isEmbedded == false)) {
                System.Management.ManagementBaseObject inParams = null;
                inParams = PrivateLateBoundObject.GetMethodParameters("UserControlService");
                inParams["ControlCode"] = ((System.Byte )(ControlCode));
                System.Management.ManagementBaseObject outParams = PrivateLateBoundObject.InvokeMethod("UserControlService", inParams, null);
                return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
            }
            else {
                return System.Convert.ToUInt32(0);
            }
        }
        
        // Enumerator implementation for enumerating instances of the class.
        /// <summary>
        /// 
        /// </summary>
        public class ServiceCollection : object, ICollection {
            
            private ManagementObjectCollection privColObj;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceCollection"/> class.
            /// </summary>
            /// <param name="objCollection">The obj collection.</param>
            public ServiceCollection(ManagementObjectCollection objCollection) {
                privColObj = objCollection;
            }

            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
            /// </summary>
            /// <value></value>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.</returns>
            public virtual int Count {
                get {
                    return privColObj.Count;
                }
            }

            /// <summary>
            /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
            /// </summary>
            /// <value></value>
            /// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
            public virtual bool IsSynchronized {
                get {
                    return privColObj.IsSynchronized;
                }
            }

            /// <summary>
            /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
            /// </summary>
            /// <value></value>
            /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
            public virtual object SyncRoot {
                get {
                    return this;
                }
            }

            /// <summary>
            /// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
            /// </summary>
            /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
            /// <param name="index">The zero-based index in array at which copying begins.</param>
            /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
            /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
            /// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
            /// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
            public virtual void CopyTo(System.Array array, int index) {
                privColObj.CopyTo(array, index);
                int nCtr;
                for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1)) {
                    array.SetValue(new Service(((System.Management.ManagementObject)(array.GetValue(nCtr)))), nCtr);
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
            /// </returns>
            public virtual System.Collections.IEnumerator GetEnumerator() {
                return new ServiceEnumerator(privColObj.GetEnumerator());
            }
            
            /// <summary>
            /// 
            /// </summary>
            public class ServiceEnumerator : object, System.Collections.IEnumerator {
                
                private ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

                /// <summary>
                /// Initializes a new instance of the <see cref="ServiceEnumerator"/> class.
                /// </summary>
                /// <param name="objEnum">The obj enum.</param>
                public ServiceEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum) {
                    privObjEnum = objEnum;
                }

                /// <summary>
                /// Gets the current element in the collection.
                /// </summary>
                /// <value></value>
                /// <returns>The current element in the collection.</returns>
                /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
                public virtual object Current {
                    get {
                        return new Service(((System.Management.ManagementObject)(privObjEnum.Current)));
                    }
                }

                /// <summary>
                /// Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns>
                /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
                /// </returns>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
                public virtual bool MoveNext() {
                    return privObjEnum.MoveNext();
                }

                /// <summary>
                /// Sets the enumerator to its initial position, which is before the first element in the collection.
                /// </summary>
                /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
                public virtual void Reset() {
                    privObjEnum.Reset();
                }
            }
        }
        
        // TypeConverter to handle null values for ValueType properties
        /// <summary>
        /// 
        /// </summary>
        public class WMIValueTypeConverter : TypeConverter {
            
            private TypeConverter baseConverter;
            
            private System.Type baseType;

            /// <summary>
            /// Initializes a new instance of the <see cref="WMIValueTypeConverter"/> class.
            /// </summary>
            /// <param name="inBaseType">Type of the in base.</param>
            public WMIValueTypeConverter(System.Type inBaseType) {
                baseConverter = TypeDescriptor.GetConverter(inBaseType);
                baseType = inBaseType;
            }

            /// <summary>
            /// Determines whether this instance [can convert from] the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="srcType">Type of the SRC.</param>
            /// <returns>
            /// 	<c>true</c> if this instance [can convert from] the specified context; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type srcType) {
                return baseConverter.CanConvertFrom(context, srcType);
            }

            /// <summary>
            /// Returns whether this converter can convert the object to the specified type, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <param name="destinationType">A <see cref="T:System.Type"></see> that represents the type you want to convert to.</param>
            /// <returns>
            /// true if this converter can perform the conversion; otherwise, false.
            /// </returns>
            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType) {
                return baseConverter.CanConvertTo(context, destinationType);
            }

            /// <summary>
            /// Converts the given object to the type of this converter, using the specified context and culture information.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"></see> to use as the current culture.</param>
            /// <param name="value">The <see cref="T:System.Object"></see> to convert.</param>
            /// <returns>
            /// An <see cref="T:System.Object"></see> that represents the converted value.
            /// </returns>
            /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                return baseConverter.ConvertFrom(context, culture, value);
            }

            /// <summary>
            /// Creates the instance.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="dictionary">The dictionary.</param>
            /// <returns></returns>
            public override object CreateInstance(System.ComponentModel.ITypeDescriptorContext context, System.Collections.IDictionary dictionary) {
                return baseConverter.CreateInstance(context, dictionary);
            }

            /// <summary>
            /// Returns whether changing a value on this object requires a call to <see cref="M:System.ComponentModel.TypeConverter.CreateInstance(System.Collections.IDictionary)"></see> to create a new value, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <returns>
            /// true if changing a property on this object requires a call to <see cref="M:System.ComponentModel.TypeConverter.CreateInstance(System.Collections.IDictionary)"></see> to create a new value; otherwise, false.
            /// </returns>
            public override bool GetCreateInstanceSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetCreateInstanceSupported(context);
            }

            /// <summary>
            /// Gets the properties.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="value">The value.</param>
            /// <param name="attributeVar">The attribute var.</param>
            /// <returns></returns>
            public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, System.Attribute[] attributeVar) {
                return baseConverter.GetProperties(context, value, attributeVar);
            }

            /// <summary>
            /// Returns whether this object supports properties, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <returns>
            /// true if <see cref="M:System.ComponentModel.TypeConverter.GetProperties(System.Object)"></see> should be called to find the properties of this object; otherwise, false.
            /// </returns>
            public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetPropertiesSupported(context);
            }

            /// <summary>
            /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be null.</param>
            /// <returns>
            /// A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"></see> that holds a standard set of valid values, or null if the data type does not support a standard set of values.
            /// </returns>
            public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValues(context);
            }

            /// <summary>
            /// Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"></see> is an exclusive list of possible values, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <returns>
            /// true if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"></see> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"></see> is an exhaustive list of possible values; false if other values are possible.
            /// </returns>
            public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesExclusive(context);
            }

            /// <summary>
            /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <returns>
            /// true if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues"></see> should be called to find a common set of values the object supports; otherwise, false.
            /// </returns>
            public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context) {
                return baseConverter.GetStandardValuesSupported(context);
            }

            /// <summary>
            /// Converts the given value object to the specified type, using the specified context and culture information.
            /// </summary>
            /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that provides a format context.</param>
            /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"></see>. If null is passed, the current culture is assumed.</param>
            /// <param name="value">The <see cref="T:System.Object"></see> to convert.</param>
            /// <param name="destinationType">The <see cref="T:System.Type"></see> to convert the value parameter to.</param>
            /// <returns>
            /// An <see cref="T:System.Object"></see> that represents the converted value.
            /// </returns>
            /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
            /// <exception cref="T:System.ArgumentNullException">The destinationType parameter is null. </exception>
            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType) {
                if ((baseType.BaseType == typeof(System.Enum))) {
                    if ((value.GetType() == destinationType)) {
                        return value;
                    }
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return  "NULL_ENUM_VALUE" ;
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((baseType == typeof(bool)) 
                            && (baseType.BaseType == typeof(System.ValueType)))) {
                    if ((((value == null) 
                                && (context != null)) 
                                && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                        return "";
                    }
                    return baseConverter.ConvertTo(context, culture, value, destinationType);
                }
                if (((context != null) 
                            && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false))) {
                    return "";
                }
                return baseConverter.ConvertTo(context, culture, value, destinationType);
            }
        }
        
        // Embedded class to represent WMI system Properties.
        /// <summary>
        /// 
        /// </summary>
        [TypeConverter(typeof(System.ComponentModel.ExpandableObjectConverter))]
        public class ManagementSystemProperties {
            
            private System.Management.ManagementBaseObject PrivateLateBoundObject;

            /// <summary>
            /// Initializes a new instance of the <see cref="ManagementSystemProperties"/> class.
            /// </summary>
            /// <param name="ManagedObject">The managed object.</param>
            public ManagementSystemProperties(System.Management.ManagementBaseObject ManagedObject) {
                PrivateLateBoundObject = ManagedObject;
            }

            /// <summary>
            /// Gets the GENUS.
            /// </summary>
            /// <value>The GENUS.</value>
            [Browsable(true)]
            public int GENUS {
                get {
                    return ((int)(PrivateLateBoundObject["__GENUS"]));
                }
            }

            /// <summary>
            /// Gets the CLASS.
            /// </summary>
            /// <value>The CLASS.</value>
            [Browsable(true)]
            public string CLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__CLASS"]));
                }
            }

            /// <summary>
            /// Gets the SUPERCLASS.
            /// </summary>
            /// <value>The SUPERCLASS.</value>
            [Browsable(true)]
            public string SUPERCLASS {
                get {
                    return ((string)(PrivateLateBoundObject["__SUPERCLASS"]));
                }
            }

            /// <summary>
            /// Gets the DYNASTY.
            /// </summary>
            /// <value>The DYNASTY.</value>
            [Browsable(true)]
            public string DYNASTY {
                get {
                    return ((string)(PrivateLateBoundObject["__DYNASTY"]));
                }
            }

            /// <summary>
            /// Gets the RELPATH.
            /// </summary>
            /// <value>The RELPATH.</value>
            [Browsable(true)]
            public string RELPATH {
                get {
                    return ((string)(PrivateLateBoundObject["__RELPATH"]));
                }
            }

            /// <summary>
            /// Gets the PROPERT y_ COUNT.
            /// </summary>
            /// <value>The PROPERT y_ COUNT.</value>
            [Browsable(true)]
            public int PROPERTY_COUNT {
                get {
                    return ((int)(PrivateLateBoundObject["__PROPERTY_COUNT"]));
                }
            }

            /// <summary>
            /// Gets the DERIVATION.
            /// </summary>
            /// <value>The DERIVATION.</value>
            [Browsable(true)]
            public string[] DERIVATION {
                get {
                    return ((string[])(PrivateLateBoundObject["__DERIVATION"]));
                }
            }

            /// <summary>
            /// Gets the SERVER.
            /// </summary>
            /// <value>The SERVER.</value>
            [Browsable(true)]
            public string SERVER {
                get {
                    return ((string)(PrivateLateBoundObject["__SERVER"]));
                }
            }

            /// <summary>
            /// Gets the NAMESPACE.
            /// </summary>
            /// <value>The NAMESPACE.</value>
            [Browsable(true)]
            public string NAMESPACE {
                get {
                    return ((string)(PrivateLateBoundObject["__NAMESPACE"]));
                }
            }

            /// <summary>
            /// Gets the PATH.
            /// </summary>
            /// <value>The PATH.</value>
            [Browsable(true)]
            public string PATH {
                get {
                    return ((string)(PrivateLateBoundObject["__PATH"]));
                }
            }
        }
    }
}
