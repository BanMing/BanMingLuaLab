-- collectgarbage_test.lua
-- @Author      : BanMing
-- @Date        : 4/5/2020, 2:37:49 PM
-- @Description :https://www.runoob.com/lua/lua-garbage-collection.html

local mymetatable = {"sss", "aaaa"}

-- 以kb为单位返回lua的内存数
print(collectgarbage("count"))

mymetatable = nil

print(collectgarbage("count"))

-- 做一次完整的垃圾回收
print(collectgarbage("collect"))

print(collectgarbage("count"))
