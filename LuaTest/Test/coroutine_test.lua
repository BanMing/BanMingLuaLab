-- coroutine_test.lua
-- @Author      : BanMing 
-- @Date        : 4/3/2019, 6:50:02 PM
-- @Description : https://www.rishiqing.com/share/doc/NzkwMzU5NQ==?version=reditor


-- 协成
local coroutine = coroutine
local coFunc = function(i)
    print(i)
end
local co = coroutine.create(coFunc)

print(coroutine.status(co))

coroutine.resume(co, 1)

print(coroutine.status(co))

print("---------------------------")

co = coroutine.wrap(coFunc)

co(2)

print("---------------------------")

co2 =
    coroutine.create(
    function()
        for i = 1, 10 do
            print(i)
            if i == 2 then
                print(coroutine.status(co2)) --running
                print(coroutine.running()) --thread:XXXXXX
            end
            coroutine.yield()
        end
    end
)

coroutine.resume(co2) --1
coroutine.resume(co2) --2
coroutine.resume(co2) --3

print(coroutine.status(co2)) -- suspended
print(coroutine.running())

print("----------")