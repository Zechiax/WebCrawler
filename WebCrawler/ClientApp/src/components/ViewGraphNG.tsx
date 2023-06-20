import React from 'react';
import { DataSet, Network } from 'vis-network/standalone/esm/vis-network';

interface INode {
    id: string;
    label: string;
    color: string;
    started?: string;
    fixed?: {
        x: boolean,
        y: boolean
    };
}

interface IEdge {
    id: string;
    from: string;
    to: string;
}

interface IGraphData {
    nodes: INode[];
    edges: IEdge[];
}

interface IState {
    graphData: IGraphData;
    graphsIds: number[];
}


class ViewGraphNG extends React.Component<{}, IState> {
    private graphRef = React.createRef<HTMLDivElement>();
    private network?: Network;

    constructor(props: {}) {
        super(props);
        this.state = {
            graphData: {
                nodes: [],
                edges: []
            },
            graphsIds: [10, 11]
        };
    }


    componentDidMount() {
        this.getGraphAsync("Record/livegraph/domains/");
    }

    componentDidUpdate(prevProps: {}, prevState: IState) {
        if (prevState.graphData !== this.state.graphData) {
            this.renderGraph();
        }
    }

    async getGraphAsync(urlbase: string) {
        const graphsJson = [];

        for (const id of this.state.graphsIds) {
            const response = await fetch(urlbase + id);

            if (response.ok) {
                const graphJson = await response.json();
                graphJson["websiteRecordId"] = id;
                graphsJson.push(graphJson);
            }
        }

        const graphData = await this.convertGraphsJsonToVisJsonAsync(graphsJson);
        this.setState({graphData});
    }

    async convertGraphsJsonToVisJsonAsync(graphsJson: any[]): Promise<IGraphData> {
        const graphData: IGraphData = {
            nodes: [],
            edges: []
        };

        for (const graphJson of graphsJson) {
            console.log("Fetching record for graph: " + graphJson.websiteRecordId);
            const recordForGraph = await (
                await fetch(`/Record/${graphJson.websiteRecordId}`)
            ).json();

            for (const node of graphJson.Graph) {
                const color = new RegExp(recordForGraph.crawlInfoData.regexPattern).test(node.Url)
                    ? this.stringToColour(recordForGraph.label)
                    : "orange";

                const alreadyPresentNode = graphData.nodes.find((n) => n.id === node.Url);

                if (alreadyPresentNode) {
                    if (
                        Date.parse(recordForGraph.crawlInfoData.lastExecutionData.started) >
                        Date.parse(alreadyPresentNode.started)
                    ) {
                        alreadyPresentNode.label = node.Title;
                        alreadyPresentNode.color = color;
                    }
                } else {
                    graphData.nodes.push({
                        id: node.Url,
                        label: node.Title,
                        color: color,
                        started: recordForGraph.crawlInfoData.lastExecutionData.started,
                        //fixed: { x: true, y: true } // This node will be static
                    });
                }

                for (const neighbourUrl of node.Neighbours) {
                    // We generate random ids for the edges
                    const id = Math.random().toString(36).substr(2, 9);
                    graphData.edges.push({
                        id: `${id}`,
                        from: node.Url,
                        to: neighbourUrl,
                    });
                }
            }
        }

        return graphData;
    }

    stringToColour = (str: string) => {
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            // tslint:disable-next-line:no-bitwise
            hash = str.charCodeAt(i) + ((hash << 5) - hash);
        }
        let colour = '#';
        for (let i = 0; i < 3; i++) {
            // tslint:disable-next-line:no-bitwise
            const value = (hash >> (i * 8)) & 0xFF;
            colour += ('00' + value.toString(16)).substr(-2);
        }
        return colour;
    }

    componentWillUnmount() {
        if (this.network) {
            this.network.destroy();
        }
    }

    renderGraph() {
        // create an array with nodes
        const nodes = new DataSet<INode>(this.state.graphData.nodes);

        // create an array with edges
        const edges = new DataSet<IEdge>(this.state.graphData.edges);

        // provide the data in the vis format
        const data = {
            nodes: nodes,
            edges: edges
        };

        const options = {
            physics: {
                solver: 'repulsion',
                repulsion: {
                    centralGravity: 0.0,
                    springLength: 220,
                    springConstant: 0.05,
                    nodeDistance: 100,
                    damping: 0.09
                },
                stabilization: {
                    enabled: true,
                    iterations: 50,
                    //updateInterval: 50,
                    onlyDynamicEdges: false,
                    fit: true
                }
            },
            layout: {
                hierarchical: false
            },
            edges: {
                smooth: false,
                color: "#000000"
            },
            height: "1000px"
        };

        // initialize your network!
        this.network = new Network(this.graphRef.current!, data, options);
    }

    render() {
        return <div ref={this.graphRef} style={{height: "1000px"}} />;
    }
}

export default ViewGraphNG;