using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epilepsy
{
    class Type
    {
        /* public  const int ADVERTISEMENT = 1;
         public const int JOIN_REQUEST = 2;
         public const int JOIN_RESPONSIVE = 3;
         public const int ASK_DATA_REQUEST = 8;*/

        public const int  REQUEST_NUM = 6;
        public const byte REPLY_PRE = 0xFF;
        public const byte ChARGING = 0;
        public const byte DISCHARGING = 1;

        public enum ConnectState
        {
            STATE_DISCONNECT=0,
            STATE_CONNECT 
        };

        public enum Reply
        {
            REP_CONFIRM = 0,
            REP_STIMULATE,
            REP_CHARGE,
            REP_EEGDATA,
            REP_RESEND,
            REP_ALIVE,         
        };

        public enum Request
        {
            REQ_CONNECT = 0,
            REQ_STIMULATE,
            REQ_CHARGE,
            REQ_EEGDATA,
            REQ_RESEND,
            KEEP_LIVE
        };

        public enum ReqState
        {
            SEND_REQ_CONNECT =0,
            SEND_KEEP_LIVE,
            SEND_REQ_EEGDATA
        };
    }
}
