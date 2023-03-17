#!/usr/bin/env bash
# CCModManager install script bundled with Linux builds.

SRCDIR="$(dirname "$(realpath "$0")")"
cd "$SRCDIR" || exit 1

echo "CCModManager exists at $SRCDIR"
echo "For a full installation experience, this script will set up the following:"

DESKTOPSRCFILE="$SRCDIR/ccmodmanager.desktop"
BINSRCFILE="$SRCDIR/ccmodmanager"
ICONSRCFILE="$SRCDIR/ccmodmanager.png"

if [ "$EUID" -ne 0 ]; then
    if [ -n "$XDG_DATA_HOME" ]; then
        APPSDIR="$XDG_DATA_HOME/applications"
    else
        APPSDIR="$HOME/.local/share/applications"
    fi
    DESKTOPFILE="$APPSDIR/CCModManager.desktop"
    echo "- A desktop file will be created at $DESKTOPFILE"
    echo "- The CCLoader scheme handler will be registered for your user"
    echo
    echo "If you want to install CCModManager system-wide, it is recommended to copy CCModManager to /opt/ccmodmanager/, fix permissions and run install.sh as root."

else
    APPSDIR="/usr/share/applications"
    DESKTOPFILE="/usr/share/applications/CCModManager.desktop"
    BINFILE="/usr/bin/ccmodmanager"
    ICONFILE="/usr/share/icons/hicolor/128x128/apps/ccmodmanager.png"
    echo "- CCModManager.desktop will be copied to $DESKTOPFILE"
    echo "- A symlink to ccmodmanager.sh will be created at $BINFILE"
    echo "- ccmodmanager.png will be copied to $ICONFILE"
    echo "- The CCModManager scheme handler will be registered for everyone"
    echo
    echo "CCModManager will be installed system-wide. Please make sure that permissions are set up properly."
fi

echo
read -p "Do you want to continue? y/N: " answer
case ${answer:0:1} in
    y|Y )
    ;;
    * )
        exit 2
    ;;
esac

if [ "$EUID" -ne 0 ]; then
    echo "Creating $DESKTOPFILE"
    mkdir -p "$(dirname "$DESKTOPFILE")"
    rm -f "$DESKTOPFILE"
    cat "$DESKTOPSRCFILE" \
    | sed "s/Exec=ccmodmanager/Exec=\"$(echo "$BINSRCFILE" | sed 's_/_\\/_g')\"/" \
    | sed "s/Icon=ccmodmanager/Icon=$(echo "$ICONSRCFILE" | sed 's_/_\\/_g')/" \
    > "$DESKTOPFILE"

else
    echo "Creating $DESKTOPFILE"
    mkdir -p "$(dirname "$DESKTOPFILE")"
    rm -f "$DESKTOPFILE"
    cp "$DESKTOPSRCFILE" "$DESKTOPFILE"
    chmod a+rx "$DESKTOPFILE"

    echo "Creating $BINFILE"
    mkdir -p "$(dirname "$BINFILE")"
    rm -f "$BINFILE"
    ln -s "$BINSRCFILE" "$BINFILE"
    chmod a+rx "$BINFILE"

    echo "Creating $ICONFILE"
    mkdir -p "$(dirname "$ICONFILE")"
    rm -f "$ICONFILE"
    cp "$ICONSRCFILE" "$ICONFILE"
    chmod a+r "$ICONFILE"
fi

# echo "Registering  scheme handler"
# xdg-mime default "$DESKTOPFILE" "x-scheme-handler/everest"
echo "Updating desktop database"
update-desktop-database "$APPSDIR"

echo "Done!"
