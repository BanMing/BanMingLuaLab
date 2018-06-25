utils={}

--计算数值有几个零及除去零后的数
function utils.ClaZeroCount(number)  
    if number == 0 then
        return 0,0
    end
    if type(number) == "string" then 
        number = tonumber(number)
    end  
    local zeroCount = 0
    local diviedNum = number
    funToCal = function() 
        if diviedNum == 0 or zeroCount >= 4 then
            return
        end
       -- print(type(number))  
        if type(number) == "userdata" then     
            if MyUnityTool.Compare(diviedNum,10) > 0 then
                diviedNum = diviedNum - diviedNum % 10
            end
            if MyUnityTool.Mod(diviedNum,10) == 0 then
                zeroCount = zeroCount + 1
                if type(number) == "userdata" then       
                    diviedNum = math.floor(MyUnityTool.Divide(diviedNum,10))
                elseif type(number) == "number" then 
                    diviedNum = math.floor(diviedNum/10)
                end
                return funToCal(diviedNum)
            end
        elseif type(number) == "number" then 
            if diviedNum > 10 then
                diviedNum = diviedNum - diviedNum % 10
            end
            if math.mod(diviedNum,10) == 0 then
                zeroCount = zeroCount + 1
                needNum = diviedNum/10
                if type(number) == "userdata" then       
                    diviedNum = math.floor(MyUnityTool.Divide(diviedNum,10))
                elseif type(number) == "number" then 
                    local temp=diviedNum/10
                    diviedNum =temp
                    -- diviedNum = math.floor(diviedNum/10)
                end
                return funToCal(diviedNum)
            end
        end
    end
    funToCal()
    funToCal = 1
    return zeroCount
end

function utils.GetPreciseDecimal(nNum, n)
    if type(nNum) ~= "number" then
        return nNum;
    end
    n = n or 0;
    n = math.floor(n)
    if n < 0 then
        n = 0;
    end
    local nDecimal = 10 ^ n
    local nTemp = math.floor(nNum * nDecimal);
    local nRet = nTemp / nDecimal;
    return nRet;
end


function utils.GetNowNumber(score)
    local zeroCount= utils.ClaZeroCount(score)
    local noZeroNum=tonumber(score)/math.pow(10,zeroCount)
    noZeroNum=utils.GetPreciseDecimal(noZeroNum,1)
    local str = ""
    if zeroCount < 4 then
        str = tostring(score)
    -- elseif zeroCount == 3 then
    --     str = tostring(noZeroNum) .. "千"
    elseif zeroCount == 4 then
        str = tostring(noZeroNum) .. "万"
    elseif zeroCount > 4 and zeroCount <8 then
        str = tostring(noZeroNum*10^(zeroCount - 4)) .. "万"
    elseif zeroCount >7 then 
        str =tostring(noZeroNum*10^(zeroCount - 7)) .. "亿"
    end
    return str
end

-- print(utils.GetNowNumber(10000))
print(utils.GetNowNumber(11500))
print(11/10)