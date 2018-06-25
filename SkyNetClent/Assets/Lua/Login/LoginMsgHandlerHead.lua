require("Login/LoginMsgHandler")
require("Protol.Base_pb")

LoginMsgHandler.S2CMsgId = {
    S2CLogin=1,
    S2CLoginSuccess=2
}

LoginMsgHandler.S2CMsgHandler = {
    [1] = "S2CLogin",
    [2] = "S2CLoginSuccess"
}
LoginMsgHandler.C2SMsgId={
    SendLogin=1
}

BigMsgId.Login = 1
BigMsgHandler[BigMsgId.Login] = LoginMsgHandler:new()
