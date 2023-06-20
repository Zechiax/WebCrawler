import React from 'react';
import { DataSet, Network } from 'vis-network/standalone/esm/vis-network';

interface INode {
    id: number;
    label: string;
}

interface IEdge {
    id: string;
    from: number;
    to: number;
}

class ViewGraphNG extends React.Component {
    private graphRef = React.createRef<HTMLDivElement>();

    componentDidMount() {
        // create an array with nodes
        const nodes = new DataSet<INode>([
            {id: 1, label: 'Node 1'},
            {id: 2, label: 'Node 2'},
            {id: 3, label: 'Node 3'}
        ]);

        // create an array with edges
        const edges = new DataSet<IEdge>([
            {id: '1to2', from: 1, to: 2},
            {id: '2to3', from: 2, to: 3}
        ]);

        // provide the data in the vis format
        const data = {
            nodes: nodes,
            edges: edges
        };

        const options = {
            layout: {
                hierarchical: false
            },
            edges: {
                color: "#000000"
            },
            height: "1000px"
        };

        // initialize your network!
        new Network(this.graphRef.current!, data, options);
    }

    render() {
        return <div ref={this.graphRef} style={{height: "1000px"}} />;
    }
}

export default ViewGraphNG;
