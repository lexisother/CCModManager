local ui, uiu, uie = require("ui").quick()
local utils = require("utils")
local threader = require("threader")

local scene = {
    name = "Benchmark Scene"
}


local root = uie.column({
    uie.topbar({
        { "File", {
            { "New" },
            { },
            { "Open" },
            { "Recent", {
                { "Totally" },
                { "Not A Dummy" },
                { "Nested Submenu", {
                    { "Totally" },
                    { "Not A Dummy" },
                    { "Nested Submenu", {
                        { "Totally" },
                        { "Not A Dummy" },
                        { "Nested Submenu", {
                            { "Totally" },
                            { "Not A Dummy" },
                            { "Nested Submenu" }
                        }}
                    }}
                }}
            }},
            { },
            { "Save" },
            { "Save As..." },
            { },
            { "Settings" },
            { },
            { "Quit", love.event.quit }
        }},

        { "Edit", {
            { "Undo" },
            { "Redo" }
        }},

        { "Map", {
            { "Stylegrounds" },
            { "Metadata" },
            { "Save Map Image" }
        }},

        { "Room", {
            { "Add" },
            { "Configure" }
        }},

        { "Help", {
            { "Update" },
            { "About" }
        }},

        { "Debug", {
            { "Uhh" }
        }},
    }),

    uie.image("header"),

    uie.paneled.row({

        uie.scrollbox(
            uie.list(
                uiu.map(uiu.listRange(2000, 1, -1), function(i)
                    return { text = string.format("%i%s", i, i % 7 == 0 and " (something)" or ""), data = i }
                end)
            ):with({
                grow = false
            }):with(uiu.fillWidth):with(function(list)
                list.selected = list.children[1] or false
            end):as("versions")
        ):with(uiu.fillWidth(4.25)):with(uiu.fillHeight),

        uie.scrollbox(
            uie.list(
                uiu.map(uiu.listRange(2000, 1, -1), function(i)
                    return { text = string.format("%i%s", i, i % 7 == 0 and " (something)" or ""), data = i }
                end)
            ):with({
                grow = false
            }):with(uiu.fillWidth):with(function(list)
                list.selected = list.children[1] or false
            end):as("versions")
        ):with(uiu.fillWidth(4.25)):with(uiu.fillHeight):with(uiu.at(0.25 + 8)),

        uie.group({

            uie.window("Windowception",
                uie.panel({
                    uie.scrollbox(
                        uie.group({
                            uie.window("Child 1", uie.panel({ uie.label("Oh no") })):with({ x = 10, y = 10}),
                            uie.window("Child 2", uie.panel({ uie.label("Oh no two") })):with({ x = 30, y = 30})
                        }):with({ width = 200, height = 400 })
                    ):with({ width = 200, height = 200 })
                }):with({
                    style = {
                        padding = 0
                    }
                })
            ):with({ x = 20, y = 100 }),

            uie.window("Hello, World!",
                uie.paneled.column({
                    uie.label("This is a big label.", ui.fontBig),

                    -- Labels use Löve2D Text objects under the hood.
                    uie.label({ { 1, 1, 1 }, "This is a ", { 1, 0, 0 }, "colored", { 0, 1, 1 }, " mono-font label."}, ui.fontMono),

                    -- Multi-line labels aren't subjected to the parent element's spacing property.
                    uie.label("This is a two-line label.\nThe following label is updated dynamically."),

                    -- Dynamically updated label.
                    uie.label():with({
                        update = function(el)
                            el.text = "FPS: " .. love.timer.getFPS()
                        end
                    }),

                    uie.button("This is a button.", function(btn)
                        btn.counter = btn.counter + 1
                    end):with({
                        getCounter = function(self)
                            return self._counter or 0
                        end,

                        setCounter = function(self, value)
                            self._counter = value
                            self.text = "Pressed " .. tostring(value) .. " time" .. (value == 1 and "" or "s")
                        end
                    }):as("counterButton"),

                    uie.button("Disabled"):with({ enabled = false }),

                    uie.button("Useless"),

                    uie.label("Select an item from the list below."):as("selected"),
                    uie.list(uiu.map(uiu.listRange(1, 3), function(i)
                        return { text = string.format("Item %i!", i), data = i }
                    end), function(list, item)
                        list.parent:findChild("selected").text = "Selected " .. tostring(item)
                    end)

                })
            ):with({ x = 200, y = 50 }):as("test"),

        }):with(uiu.fillWidth(0.5)):with(uiu.fillHeight):with(uiu.at(0.5 + 8)),

    }):with(uiu.fillWidth):with(uiu.fillHeight(true)),

})
scene.root = root


function scene.load()

end


function scene.enter()

end


return scene
