BaseHandler=class("BaseHandler")

function BaseHandler:OnMessage(smallId, buffer)
   self[self.S2CMsgHandler[smallId]](self,buffer)
end