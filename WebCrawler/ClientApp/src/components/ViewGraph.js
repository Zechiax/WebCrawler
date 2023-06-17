import React, { Component } from "react";
import { useLocation } from "react-router";
import Button from "react-bootstrap/Button";
import * as d3 from "d3";

export const ViewGraph = (props) => {
  const location = useLocation();
  return (
    <ViewGraphInternal
      ids={location.state.ids}
      height="800px"
      width="100%"
      {...props}
    />
  );
};

class ViewGraphInternal extends Component {
  constructor(props) {
    super(props);
    this.state = {
      nodes: [],
      links: [],
      loading: true,
      isWebsitesView: false,
      isLiveView: false,
      currentlySelectedNodeInfo: [],
    };
  }

  renderGraph(graph) {
    const nodes = graph.nodes.map((node) => Object.create(node));
    const links = graph.links.map((link) => Object.create(link));
    this.setState({ nodes: nodes });
    this.setState({ links: links });

    const nodeInfo = d3.select(".content .nodeInfo");

    const svg = d3
      .select("svg")
      .attr("width", this.props.width)
      .attr("height", this.props.height)
      .call(
        d3.zoom().on("zoom", function (e) {
          d3.selectAll("svg g").attr("transform", e.transform);
        })
      );

    const simulation = d3
      .forceSimulation()
      .force("charge", d3.forceManyBody().strength(-5))
      .force(
        "center",
        d3.forceCenter(
          350, //TODO - calculate center somehow from svg dimension
          350
        )
      );

    // TODO: Attempt to implement dragging, but doesn't work for some reason
    svg.call(
      d3
        .drag()
        .container(svg)
        .subject((e) => simulation.find(e.x, e.y))
        .on("start", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0.3).restart();
          }

          e.subject.fx = e.subject.x;
          e.subject.fy = e.subject.y;
        })
        .on("drag", (e) => {
          e.subject.fx = e.x;
          e.subject.fy = e.y;
        })
        .on("end", (e) => {
          if (!e.active) {
            simulation.alphaTarget(0);
          }

          e.subject.fx = null;
          e.subject.fy = null;
        })
    );
    //

    const linkElements = svg
      .append("g")
      .selectAll("line")
      .data(links)
      .enter()
      .append("line")
      .attr("stroke-width", 1)
      .attr("stroke", "#E5E5E5");

    const nodeElements = svg
      .append("g")
      .selectAll("circle")
      .data(nodes)
      .enter()
      .append("circle")
      .attr("id", function (d) {
        return d.url;
      })
      .attr("class", ".node")
      .attr("r", 3)
      .attr("fill", function (d) {
        return d.color;
      })
      .on("click", function (e, d, i) {
        nodeInfo.selectAll("ul").remove();
        svg
          .selectAll("circle")
          .filter(function () {
            return d3.select(this).attr("fill") === "red";
          })
          .attr("fill", d.color);

        nodeInfo
          .append("div")
          .style("font-size", "12px")
          .append("ul")
          .append("li")
          .text("Title: " + (d.title ? d.title : "???"))
          .append("li")
          .text("Url: " + d.url)
          .append("li")
          .text("Crawl time: " + d.crawlTime)
          .append("li")
          .text(d.inWhichGraphs.map((graphId) => graphId));

        d3.select(this).attr("fill", "red");
      });

    simulation.nodes(nodes).on("tick", () => {
      nodeElements.attr("cx", (node) => node.x).attr("cy", (node) => node.y);

      linkElements
        .attr("x1", (link) => link.source.x)
        .attr("y1", (link) => link.source.y)
        .attr("x2", (link) => link.target.x)
        .attr("y2", (link) => link.target.y);
    });

    simulation.force(
      "link",
      d3
        .forceLink(links)
        .id((link) => link.url)
        .distance(20)
    );
  }

  componentDidMount() {
    this.fetchAndRerenderGraph();
  }

  fetchAndRerenderGraph() {
    this.setState({ loading: true });
    this.getGraphAsync()
      .then((graph) => this.renderGraph(graph))
      .then(this.setState({ loading: false }));
  }

  async getGraphAsync() {
    let urlBase = "";

    if (this.state.isWebsitesView) {
      urlBase = "/Record/livegraph/websites/";
    } else {
      urlBase = "/Record/livegraph/domains/";
    }

    console.log(urlBase);

    const graphsJson = [];

    console.log(this.props.ids);

    for (const id of this.props.ids) {
      const response = await fetch(urlBase + id);

      if (response.ok) {
        const graphJson = await response.json();
        graphJson["websiteRecordId"] = id;
        console.log(graphJson);
        graphsJson.push(graphJson);
      }
    }

    const graph = await ViewGraphInternal.convertGraphsJsonToD3JsonAsync(
      graphsJson
    );
    return graph;
  }

  static async convertGraphsJsonToD3JsonAsync(graphsJson) {
    const graph = {
      nodes: [],
      links: [],
    };

    for (const graphJson of graphsJson) {
      const recordForGraph = await (
        await fetch(`/Record/${graphJson.websiteRecordId}`)
      ).json();
      console.log("record for graph");
      console.log(recordForGraph);

      for (const node of graphJson.Graph) {
        // if regex doesn't match is orange
        const color = new RegExp(recordForGraph.crawlInfo.regexPattern).test(
          node.Url
        )
          ? ViewGraphInternal.stringToColour(recordForGraph.label)
          : "orange";

        const alreadyPresentNode = graph.nodes.find((n) => n.url === node.Url);

        // present but this one is newer
        if (alreadyPresentNode) {
          if (
            Date.parse(recordForGraph.crawlInfo.lastExecution.started) >
            alreadyPresentNode.started
          ) {
            console.log(alreadyPresentNode);
            // overwrite with latest data
            alreadyPresentNode.url = node.Url;
            alreadyPresentNode.title = node.Title;
            alreadyPresentNode.crawlTime = node.CrawlTime;
            alreadyPresentNode.started = Date.parse(
              recordForGraph.crawlInfo.lastExecution.started
            );

            alreadyPresentNode.color = color;
          }

          alreadyPresentNode.inWhichGraphs.push(recordForGraph.label);
        } else {
          graph.nodes.push({
            url: node.Url,
            title: node.Title,
            crawlTime: node.CrawlTime,
            started: Date.parse(recordForGraph.crawlInfo.lastExecution.started),
            inWhichGraphs: [recordForGraph.label],
            color: color,
          });
        }

        for (const neighbourUrl of node.Neighbours) {
          graph.links.push({
            source: node.Url,
            target: neighbourUrl,
            forWhichGraph: recordForGraph.label,
          });
        }
      }
    }

    return graph;
  }

  //https://stackoverflow.com/questions/3426404/create-a-hexadecimal-colour-based-on-a-string-with-javascript
  static stringToColour(str) {
    var hash = 0;
    for (var i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    var colour = "#";
    for (var i = 0; i < 3; i++) {
      var value = (hash >> (i * 8)) & 0xff;
      colour += ("00" + value.toString(16)).substr(-2);
    }
    return colour;
  }

  updateGraph() {
    // TODO: attempt for live update instead of rererending the whole graph all the time - not working
    /*     this.setState((state, props) => ({
      nodes: [
        ...nodes,
        {
          url: "borek",
        },
      ],
    }));

    let nodes = d3.selectAll("circle").data(this.state.nodes);
    const newNodesEnter = nodes
      .enter()
      .append("circle")
      .attr("id", function (d) {
        return d.url;
      })
      .attr("class", ".node")
      .attr("r", 3)
      .attr("fill", function (d) {
        return d.color;
      });

    nodes = newNodesEnter.merge(nodes);
    // */

    this.deleteGraph();
    this.fetchAndRerenderGraph();
  }

  deleteGraph() {
    d3.selectAll("circle").remove();
    d3.selectAll("line").remove();
  }

  render() {
    let loading = "";
    if (this.state.loading) {
      loading = (
        <p>
          <em>Loading ...</em>
        </p>
      );
    }

    return (
      <div className="content">
        <div
          className="nodeInfo"
          style={{
            position: "absolute",
            zIndex: 1,
          }}
        />
        <div
          style={{
            position: "absolute",
            left: "85vw",
          }}
        >
          <Button
            style={{
              width: "130px",
            }}
            variant="primary"
            onClick={(e) => {
              this.setState((state, props) => ({
                isWebsitesView: !state.isWebsitesView,
              }));

              this.deleteGraph();
              this.fetchAndRerenderGraph();
            }}
          >
            {this.state.isWebsitesView ? "Websites view" : "Domains view"}
          </Button>
          <Button
            style={{
              marginTop: "10px",
              width: "130px",
            }}
            variant="primary"
            onClick={(e) => {
              this.setState((state, props) => ({
                isLiveView: !state.isLiveView,
              }));
            }}
          >
            {this.state.isLiveView ? "Live view" : "Static view"}
          </Button>
        </div>
        {!this.state.isLiveView ? (
          <div
            style={{
              position: "fixed",
              left: "85vw",
              bottom: "2vw",
            }}
          >
            <Button
              onClick={() => {
                this.updateGraph();
              }}
            >
              Update Graph
            </Button>
          </div>
        ) : (
          <div></div>
        )}
        {loading}
        <svg
          style={{
            zIndex: 0,
          }}
        ></svg>
      </div>
    );
  }
}
