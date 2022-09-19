--[[
	Crude, but it gets the job done
]]

local csproj = io.open("amblflecasm.csproj", "rb")
if not csproj then print("Failed to open 'amblflecasm.csproj'") return end

local csData = csproj:read("*all")
csproj:close()

if not csData then print("Failed to read 'amblflecasm.csproj'") return end

local StartPos, EndPos = string.find(csData, "<TargetFrameworkVersion>", 1, true)
local Nend = string.find(csData, "</TargetFrameworkVersion>", EndPos, true)
local TargetFrameworkVersion = string.sub(csData, EndPos + 1, Nend - 1, true)

StartPos, EndPos = string.find(csData, "<LangVersion>", 1, true)
Nend = string.find(csData, "</LangVersion>", EndPos, true)
local LangVersion = string.sub(csData, EndPos + 1, Nend - 1, true)

local Final = ".NET " .. TargetFrameworkVersion .. " \\\nC# v" .. LangVersion .. "\n\nNuget Packages:\n"
local Search = "<Reference Include=\""

StartPos, EndPos = string.find(csData, Search, 1, true)
Nend = string.find(csData, ">", EndPos, true)

function string.split(split, sep) -- https://stackoverflow.com/questions/1426954/split-string-in-lua (Tweaked)
	sep = sep or "%s"

	local SplitStr = {}

	for str in string.gmatch(split, "([^"..sep.."]+)") do
		SplitStr[#SplitStr + 1] = str
	end

	return SplitStr
end

while StartPos ~= nil do
	local Sub = string.split(string.sub(csData, StartPos + #Search, Nend), ", ")

	local vIndex = -1

	for i = 1, #Sub do
		if string.find(Sub[i], "Version=") then
			vIndex = i
			break
		end
	end

	if vIndex ~= -1 then
		Final = Final .. " - " .. Sub[1] .. " " .. (string.sub(Sub[vIndex], #("Version=") + 1)) .. "\n"
	end

	StartPos, EndPos = string.find(csData, Search, Nend, true)
	Nend = string.find(csData, ">", EndPos, true)
end

local readme = io.open("readme.md", "w")

xpcall(function()
	readme:write(Final)
end, function()
	print("Failed to write 'readme.md'")
end)

readme:close()