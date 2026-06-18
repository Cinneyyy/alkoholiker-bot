rsync -rav -e ssh --exclude="vc_state/*" --exclude="msg_log/*" --exclude="log.txt" --exclude="call_stats/*" \
    ~/.config/alkoholiker/ \
    cinneyyy@192.168.5.82:/home/cinneyyy/.config/alkoholiker
