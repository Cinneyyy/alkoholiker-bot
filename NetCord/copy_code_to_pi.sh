rsync -rav -e ssh --exclude="bin/*" --exclude="obj/*" \
    ./ \
    cinneyyy@192.168.2.100:/home/cinneyyy/Desktop/Bot/
