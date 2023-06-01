export function setSortable(pageRef) {
    let list = document.getElementsByClassName("mud-treeview");
    for (let l of list) {
        Sortable.create(l, {
            handle: '.draghandle',
            //group: 'nested', 
            animation: 150,
            fallbackOnBody: true,
            swapThreshold: 0.65,
            onEnd: function (/**Event*/evt) {
                //console.log(evt.clone);

                let id = '';
                let children = evt.clone.getElementsByTagName("div");
                for (var i = 0; i < children.length; i++) {
                    var child = children[i];
                    var classNames = child.className.split(' ');
                    for (var j = 0; j < classNames.length; j++) {
                        if (classNames[j] === 'identify') {
                            id = child.innerText;
                            break;
                        }
                    }
                    if (id !== '') {
                        break;
                    }
                }
                pageRef.invokeMethodAsync('DragEnd', Number(id), evt.oldIndex, evt.newIndex);
            }
        });
    }

    //let list2 = document.getElementsByClassName("mud-treeview-item");
    //for (let l of list2) {
    //    Sortable.create(l, {
    //        handle: '.hidden-btn',
    //        animation: 150,
    //        onEnd: function (/**Event*/evt) {
    //            console.log(evt);

    //            pageRef.invokeMethodAsync('DragEnd', evt.oldIndex, evt.newIndex);
    //        }
    //    });
    //}
}