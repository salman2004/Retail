System.register(["PosApi/Extend/Triggers/OperationTriggers"], function (exports_1, context_1) {
    "use strict";
    var __extends = (this && this.__extends) || (function () {
        var extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return function (d, b) {
            extendStatics(d, b);
            function __() { this.constructor = d; }
            d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
        };
    })();
    var __moduleName = context_1 && context_1.id;
    var Triggers, ErrorAudioSoundPostOperationTrigger;
    return {
        setters: [
            function (Triggers_1) {
                Triggers = Triggers_1;
            }
        ],
        execute: function () {
            ErrorAudioSoundPostOperationTrigger = (function (_super) {
                __extends(ErrorAudioSoundPostOperationTrigger, _super);
                function ErrorAudioSoundPostOperationTrigger() {
                    return _super !== null && _super.apply(this, arguments) || this;
                }
                ErrorAudioSoundPostOperationTrigger.prototype.execute = function (options) {
                    var audio = new Audio("../../../Assets/windows-error-sound-effect-35894.mp3");
                    audio.play();
                    return Promise.resolve();
                };
                return ErrorAudioSoundPostOperationTrigger;
            }(Triggers.OperationFailureTrigger));
            exports_1("default", ErrorAudioSoundPostOperationTrigger);
        }
    };
});
//# sourceMappingURL=C:/RetailSDK/Update/RetailSDK_FinalV28/Pos/Extensions/AudioSound/TriggerHandlers/ErrorAudioSoundPostOperationTrigger.js.map